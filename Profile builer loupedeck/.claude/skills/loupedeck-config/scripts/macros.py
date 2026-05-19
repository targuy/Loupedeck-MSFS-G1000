"""Manage macros, macro-adjustments, and profile-actions.

Subcommands:
    list                       — list everything in the profile
    add-launch-app             — macroCommand that launches an executable
    add-keyboard-action        — profileAction that fires a keyboard shortcut
    add-macro-keysequence      — macroCommand that fires a sequence of keys
    remove                     — delete a macro / adjustment / profile-action by name (GUID or full action ID)

Usage:
    python macros.py list <dir>
    python macros.py add-launch-app <dir> --name "Chrome" --exe "C:/.../chrome.exe" [--args "..."] [--cwd "..."]
    python macros.py add-keyboard-action <dir> --name "Screenshot" --keys "Ctrl+Shift+S"
    python macros.py add-macro-keysequence <dir> --name "Greet" --keys "Ctrl+A" "Delete" "H" "i"
    python macros.py remove <dir> --name <GUID-or-full-action-id>

Every "add" subcommand prints the action ID you can paste into a button.
"""
from __future__ import annotations

import argparse
import re
from pathlib import Path

from _common import die, load_profile, new_guid, save_profile


# --- keyboard key encoding ------------------------------------------------

# Modifier mask values verified against the export corpus:
#   Alt+Tab            → 2       (Alt=2)
#   Alt+Shift+Tab      → 6       (Alt|Shift, so Shift=4)
#   Windows+ArrowUp    → 8       (Win=8)
#   Windows+Shift+...  → 12      (Win|Shift)
# Ctrl is *not* observed in the corpus; bit 1 is the only remaining low bit, so
# Ctrl=1 is the best inference. If you see misbehaviour, capture a real Ctrl-key
# binding via the configui and adjust.
MODIFIER_DISPLAY = {
    "ctrl": ("Ctrl", "Ctrl", 1),
    "control": ("Ctrl", "Ctrl", 1),
    "shift": ("Shift", "Shift", 4),
    "alt": ("AltOrOption", "Alt", 2),
    "altoroption": ("AltOrOption", "Alt", 2),
    "option": ("AltOrOption", "Alt", 2),
    "win": ("Windows", "Win", 8),
    "windows": ("Windows", "Win", 8),
    "cmd": ("Windows", "Win", 8),
}

# Win virtual-key codes used in real profiles.
VK = {
    "tab": 9, "enter": 13, "return": 13, "escape": 27, "esc": 27, "space": 32,
    "arrowleft": 37, "left": 37, "arrowup": 38, "up": 38, "arrowright": 39, "right": 39, "arrowdown": 40, "down": 40,
    "delete": 46, "del": 46, "backspace": 8, "insert": 45, "home": 36, "end": 35, "pageup": 33, "pagedown": 34,
    **{chr(c).lower(): c for c in range(ord("A"), ord("Z") + 1)},
    **{str(i): 0x30 + i for i in range(10)},
    **{f"f{i}": 0x70 + (i - 1) for i in range(1, 13)},  # F1..F12
}

# PS/2 set-1 scan codes (low byte, without 0xE0 extension prefix).
# Verified against real profile corpus: Tab→15, ArrowUp→72, ArrowLeft→75,
# ArrowRight→77, ArrowDown→80. This is the trailing field of keyboardKey.
SCANCODE = {
    # numbers
    27: 1,                              # Esc
    8: 14, 9: 15, 13: 28, 32: 57,       # Backspace, Tab, Enter, Space
    **{0x30 + i: (2 + i) for i in range(1, 10)},  # 1..9
    0x30: 11,                                       # 0
    # letters
    65: 30, 66: 48, 67: 46, 68: 32, 69: 18, 70: 33, 71: 34, 72: 35, 73: 23,
    74: 36, 75: 37, 76: 38, 77: 50, 78: 49, 79: 24, 80: 25, 81: 16, 82: 19,
    83: 31, 84: 20, 85: 22, 86: 47, 87: 17, 88: 45, 89: 21, 90: 44,
    # F-keys
    0x70: 59, 0x71: 60, 0x72: 61, 0x73: 62, 0x74: 63, 0x75: 64,
    0x76: 65, 0x77: 66, 0x78: 67, 0x79: 68, 0x7A: 87, 0x7B: 88,
    # navigation
    37: 75, 38: 72, 39: 77, 40: 80,       # Arrow L/U/R/D
    45: 82, 46: 83, 36: 71, 35: 79, 33: 73, 34: 81,  # Insert/Delete/Home/End/PgUp/PgDn
}

DISPLAY_KEY = {v: (k[0].upper() + k[1:] if not k.startswith("f") else f"F{k[1:]}") for k, v in VK.items()}
# Map a few canonical display names
DISPLAY_KEY[37] = "ArrowLeft"; DISPLAY_KEY[38] = "ArrowUp"; DISPLAY_KEY[39] = "ArrowRight"; DISPLAY_KEY[40] = "ArrowDown"
DISPLAY_KEY[46] = "Delete"; DISPLAY_KEY[8] = "Backspace"; DISPLAY_KEY[9] = "Tab"; DISPLAY_KEY[27] = "Escape"
DISPLAY_KEY[13] = "Enter"; DISPLAY_KEY[32] = "Space"

# Marker token observed in real keyboardKey strings — keep literal.
SEP = "67896332"
SUB_SEP = "#¤%&+?"


def encode_keyboard_key(combo: str) -> str:
    parts = [p.strip() for p in combo.split("+") if p.strip()]
    if not parts:
        die(f"empty key combo: {combo!r}")
    *mods_in, key = parts
    canon_mods = []
    display_mods = []
    mod_mask = 0
    for m in mods_in:
        key_lc = m.lower()
        if key_lc not in MODIFIER_DISPLAY:
            die(f"unknown modifier: {m!r}")
        canon, disp, bit = MODIFIER_DISPLAY[key_lc]
        canon_mods.append(canon); display_mods.append(disp); mod_mask |= bit
    key_lc = key.lower()
    vk = VK.get(key_lc)
    if vk is None:
        die(f"unknown key: {key!r}. Known: {sorted(set(VK))[:20]}...")
    sc = SCANCODE.get(vk)
    if sc is None:
        die(f"no scan code mapping for VK {vk} (key {key!r}); add it to SCANCODE in macros.py")
    disp_key = DISPLAY_KEY.get(vk, key.upper())
    canonical = "+".join(canon_mods + [disp_key])
    display = "+".join(display_mods + [disp_key])
    # Layout:  <canonical>___67896332___<display>___win-<vk>#¤%&+?<mod_mask>#¤%&+?67896332#¤%&+?<scancode>
    return f"{canonical}___{SEP}___{display}___win-{vk}{SUB_SEP}{mod_mask}{SUB_SEP}{SEP}{SUB_SEP}{sc}"


# --- subcommands ----------------------------------------------------------

def cmd_list(args, profile: dict) -> None:
    for entry in profile.get("macroCommands") or []:
        actions = entry.get("actions") or []
        first = actions[0] if actions else "(empty)"
        print(f"  Macro             {entry['name']}  {entry['displayName']!r}  → {first[:80]}")
    for entry in profile.get("macroAdjustments") or []:
        print(f"  MacroAdjustment   {entry['name']}  {entry['displayName']!r}  L={len(entry.get('actionsLeft', []))}, R={len(entry.get('actionsRight', []))}")
    for entry in profile.get("profileActions") or []:
        keys = ((entry.get("actionParameters") or {}).get("parameters") or {}).get("keyboardKey", "?")
        canonical = keys.split("___", 1)[0]
        print(f"  ProfileAction     {entry['name']}  {entry['displayName']!r}  keys={canonical}")


def cmd_add_launch_app(args, profile: dict) -> str:
    macro_id = new_guid()
    action = f"$@Generic___@ExecuteApplication___{args.exe}||{args.args_or_empty}||{args.cwd_or_empty}"
    profile.setdefault("macroCommands", []).append({
        "$type": "Loupedeck.Service.ApplicationProfileMacroCommand, LoupedeckService",
        "isCommand": True,
        "name": macro_id,
        "displayName": args.name,
        "description": "",
        "groupName": "",
        "superGroupName": "@macro",
        "supportedOs": "All",
        "supportedModes": ["system"],
        "showAsSingleAction": True,
        "actionEditorCommands": [],
        "isMultiState": False,
        "actions": [action],
    })
    return f"$@Generic___@Macro___{macro_id}"


def cmd_add_keyboard_action(args, profile: dict) -> str:
    pa_guid = new_guid()
    name = f"$@Generic___@ProfileAction___{pa_guid}"
    profile.setdefault("profileActions", []).append({
        "$type": "Loupedeck.Service.ApplicationProfileCommand, LoupedeckService",
        "isCommand": True,
        "name": name,
        "templateActionName": "$@Generic___@KeyboardKey",
        "actionParameters": {
            "$type": "Loupedeck.ActionEditorActionParameters, PluginApi",
            "parameters": {
                "$type": "Loupedeck.StringDictionaryNoCase, PluginApi",
                "keyboardKey": encode_keyboard_key(args.keys),
            },
            "count": 1,
        },
        "displayName": args.name,
        "description": "Activate a keyboard shortcut with a single press",
        "groupName": "",
        "superGroupName": "@macro",
        "isProfileAction": True,
        "isMultiState": False,
        "isResetCommand": False,
        "adjustmentName": None,
        "states": None,
    })
    return name


def cmd_add_macro_keysequence(args, profile: dict) -> str:
    """A macroCommand whose actions[] are a sequence of keyboard-key invocations.

    Each step is a transient profileAction-style command stored inline. Loupedeck
    accepts plain action IDs in macroCommands.actions[], so we emit one
    `$@Generic___@ProfileAction___<GUID>` per step and register those as profile-
    actions too (matching the pattern seen in exports).
    """
    macro_id = new_guid()
    action_ids: list[str] = []
    for combo in args.keys:
        pa = type("ns", (), {"name": f"step {combo}", "keys": combo})()
        action_ids.append(cmd_add_keyboard_action(pa, profile))
    profile.setdefault("macroCommands", []).append({
        "$type": "Loupedeck.Service.ApplicationProfileMacroCommand, LoupedeckService",
        "isCommand": True,
        "name": macro_id,
        "displayName": args.name,
        "description": "",
        "groupName": "",
        "superGroupName": "@macro",
        "supportedOs": "All",
        "supportedModes": ["system"],
        "showAsSingleAction": True,
        "actionEditorCommands": [],
        "isMultiState": False,
        "actions": action_ids,
    })
    return f"$@Generic___@Macro___{macro_id}"


def cmd_remove(args, profile: dict) -> None:
    name = args.name
    removed = 0
    for bucket in ("macroCommands", "macroAdjustments", "profileActions"):
        entries = profile.get(bucket) or []
        keep = []
        for e in entries:
            if e.get("name") == name:
                removed += 1
                continue
            keep.append(e)
        profile[bucket] = keep
    if not removed:
        die(f"nothing matched name {name!r}")
    print(f"  removed {removed} entries with name {name!r}")


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)

    common_dir = lambda p: p.add_argument("profile_dir")
    common_dry = lambda p: p.add_argument("--dry-run", action="store_true")

    p = sub.add_parser("list"); common_dir(p)

    p = sub.add_parser("add-launch-app"); common_dir(p); common_dry(p)
    p.add_argument("--name", required=True)
    p.add_argument("--exe", required=True)
    p.add_argument("--args", dest="args_or_empty", default="")
    p.add_argument("--cwd", dest="cwd_or_empty", default="")

    p = sub.add_parser("add-keyboard-action"); common_dir(p); common_dry(p)
    p.add_argument("--name", required=True)
    p.add_argument("--keys", required=True, help='e.g. "Ctrl+Shift+P"')

    p = sub.add_parser("add-macro-keysequence"); common_dir(p); common_dry(p)
    p.add_argument("--name", required=True)
    p.add_argument("--keys", nargs="+", required=True)

    p = sub.add_parser("remove"); common_dir(p); common_dry(p)
    p.add_argument("--name", required=True)

    args = ap.parse_args()
    root, profile = load_profile(args.profile_dir)

    if args.cmd == "list":
        cmd_list(args, profile)
        return

    if args.cmd == "remove":
        cmd_remove(args, profile)
    else:
        action_id = {
            "add-launch-app": cmd_add_launch_app,
            "add-keyboard-action": cmd_add_keyboard_action,
            "add-macro-keysequence": cmd_add_macro_keysequence,
        }[args.cmd](args, profile)
        print(f"  action ID: {action_id}")

    if not args.dry_run:
        save_profile(root, profile)
        print(f"  saved {root / 'ProfileInfo.json'} (backup at .json.bak)")


if __name__ == "__main__":
    main()
