"""Copy a page (and dependent macros/profile-actions/icons) from one profile to many.

Use case: you have a "Sound control" or "Windows shortcuts" page in one profile and
want the same page available in every other profile so you don't have to switch
profiles on the device.

Two strategies:

  --strategy clone-page
      Append a new touch/encoder/wheel page (fresh GUID) to a target workspace
      in each target profile. Default. Always safe (no collisions).

  --strategy replace-page
      Overwrite an existing same-named touch/encoder/wheel page in each target
      profile. Use this for keeping a "sync" page up to date across profiles.

Action IDs are copied as-is. Any macro/profileAction GUIDs they reference are
also copied (with a `<dst already has it>` check so the same macro isn't
duplicated). Icons (`.ict` / `.png` / ActionColors entries) referenced by the
page are copied too.

Usage:
    python propagate.py \\
        --src <profile_dir> --src-workspace 0 --src-page 1 --kind touch \\
        --dst <profile1_dir> --dst <profile2_dir> ... \\
        [--dst-workspace 0] \\
        [--strategy clone-page|replace-page] \\
        [--rename "Sound control"] \\
        [--dry-run]
"""
from __future__ import annotations

import argparse
import copy
import json
import re
import shutil
from pathlib import Path

from _common import die, find_mode, find_page, find_workspace, load_profile, new_guid, save_profile


ACTION_RE = re.compile(r"\$@Generic___@(Macro|MacroAdjustment|ProfileAction)___([A-F0-9]{32})")
SUBACTION_RE = re.compile(r"\$@Generic___@(Macro|MacroAdjustment|ProfileAction)___([A-F0-9]{32})")


def _collect_action_ids(controls: list, *, kind: str) -> set[str]:
    out: set[str] = set()
    if kind == "wheel":
        return out  # handled separately via parameters
    fields = (("pressAction", "fnPressAction") if kind == "touch"
              else ("pressAction", "fnPressAction", "rotateAction", "fnRotateAction"))
    for ctl in controls or []:
        for f in fields:
            v = ctl.get(f)
            if v:
                out.add(v)
    return out


def _find_macro(profile: dict, key: str, guid: str) -> dict | None:
    """key in {'Macro','MacroAdjustment','ProfileAction'}."""
    bucket = {
        "Macro": "macroCommands",
        "MacroAdjustment": "macroAdjustments",
        "ProfileAction": "profileActions",
    }[key]
    full_action_id = f"$@Generic___@{key}___{guid}"
    for entry in profile.get(bucket) or []:
        name = entry.get("name")
        if key == "ProfileAction":
            if name == full_action_id:
                return entry
        else:
            if name == guid:
                return entry
    return None


def _ensure_macros_copied(src: dict, dst: dict, action_ids: set[str]) -> list[str]:
    """For each referenced macro/profileAction in src, make sure dst has it too. Returns log lines."""
    log: list[str] = []
    queue = list(action_ids)
    visited: set[str] = set()
    while queue:
        aid = queue.pop()
        if aid in visited:
            continue
        visited.add(aid)
        m = ACTION_RE.match(aid)
        if not m:
            continue
        kind, guid = m.group(1), m.group(2)
        src_entry = _find_macro(src, kind, guid)
        if src_entry is None:
            log.append(f"  warn: {aid} referenced but not found in source profile")
            continue
        if _find_macro(dst, kind, guid) is not None:
            log.append(f"  skip {kind} {guid}: already in destination")
            continue
        bucket = {
            "Macro": "macroCommands",
            "MacroAdjustment": "macroAdjustments",
            "ProfileAction": "profileActions",
        }[kind]
        dst.setdefault(bucket, [])
        dst[bucket].append(copy.deepcopy(src_entry))
        log.append(f"  copied {kind} {guid}")
        # Recurse: nested actions (a Macro can call other action IDs).
        for sub in src_entry.get("actions") or []:
            if isinstance(sub, str):
                queue.append(sub)
    return log


def _copy_icon_assets(src_root: Path, dst_root: Path, action_ids: set[str], *, dry_run: bool) -> list[str]:
    log: list[str] = []
    for aid in action_ids:
        # filename uses the action id verbatim
        for sub, ext in (("ActionIcons", ".ict"), ("ActionImages", ".png")):
            src_f = src_root / sub / f"{aid}{ext}"
            if src_f.is_file():
                dst_dir = dst_root / sub
                dst_dir.mkdir(exist_ok=True)
                dst_f = dst_dir / src_f.name
                if dst_f.exists():
                    log.append(f"  skip asset (already present): {sub}/{src_f.name}")
                    continue
                if not dry_run:
                    shutil.copy2(src_f, dst_f)
                log.append(f"  copied asset: {sub}/{src_f.name}")
    # ActionColors (composite keys include the page GUID, so only worth copying the bare-actionId ones).
    src_ac = src_root / "ActionColors" / "ActionColors.json"
    dst_ac = dst_root / "ActionColors" / "ActionColors.json"
    if src_ac.is_file():
        with src_ac.open(encoding="utf-8") as f:
            src_map = json.load(f)
        dst_map = {}
        if dst_ac.is_file():
            with dst_ac.open(encoding="utf-8") as f:
                dst_map = json.load(f)
        added = 0
        for k, v in src_map.items():
            base = k.split("|", 1)[0]
            if base in action_ids and k not in dst_map:
                dst_map[k] = v
                added += 1
        if added and not dry_run:
            dst_ac.parent.mkdir(exist_ok=True)
            with dst_ac.open("w", encoding="utf-8") as f:
                json.dump(dst_map, f, indent=4, ensure_ascii=False)
        if added:
            log.append(f"  copied {added} ActionColors entries")
    return log


def _clone_page(src_page: dict, *, kind: str, new_name: str | None, regen_guid: bool = True) -> dict:
    new_page = copy.deepcopy(src_page)
    if regen_guid:
        new_page["name"] = new_guid()
    if new_name:
        new_page["displayName"] = new_name
    return new_page


def propagate_to(src_root: Path, src_profile: dict, src_page: dict, *,
                 kind: str, action_ids: set[str],
                 dst_path: Path, dst_workspace_idx: int,
                 strategy: str, rename: str | None,
                 dry_run: bool) -> None:
    print(f"\n=> destination: {dst_path}")
    dst_root, dst_profile = load_profile(dst_path)
    dst_ws = find_workspace(dst_profile, dst_workspace_idx)
    dst_mode = find_mode(dst_profile)

    bucket_key = {"touch": "touchPages", "encoder": "encoderPages", "wheel": "wheelPages"}[kind]
    ws_key = {"touch": "touchPageNames", "encoder": "encoderPageNames", "wheel": "wheelPageNames"}[kind]

    if strategy == "replace-page":
        target_name = rename or src_page["displayName"]
        existing = next((p for p in dst_mode[bucket_key] if p["displayName"] == target_name), None)
        if existing is None:
            print(f"  no existing page named {target_name!r} in destination; falling back to clone-page")
            strategy = "clone-page"
        else:
            # Preserve the destination GUID so any references in dst still work.
            preserved = existing["name"]
            new_page = _clone_page(src_page, kind=kind, new_name=target_name, regen_guid=False)
            new_page["name"] = preserved
            idx = dst_mode[bucket_key].index(existing)
            dst_mode[bucket_key][idx] = new_page
            print(f"  replaced existing {kind} page {target_name!r} (kept GUID {preserved})")

    if strategy == "clone-page":
        new_page = _clone_page(src_page, kind=kind, new_name=rename, regen_guid=True)
        dst_mode[bucket_key].append(new_page)
        if new_page["name"] not in dst_ws[ws_key]:
            dst_ws[ws_key].append(new_page["name"])
        print(f"  added {kind} page {new_page['displayName']!r} (new GUID {new_page['name']}) "
              f"to workspace {dst_ws['displayName']!r}")

    # Carry over referenced macros/profileActions.
    for line in _ensure_macros_copied(src_profile, dst_profile, action_ids):
        print(line)

    # Copy icon assets.
    for line in _copy_icon_assets(src_root, dst_root, action_ids, dry_run=dry_run):
        print(line)

    # Declare any non-builtin plugins from src whose actions appear here.
    BUILTIN_PLUGINS = {"@Generic", "DefaultWin", "DefaultMac", "DefaultLinux"}
    plugins_needed = set()
    for aid in action_ids:
        if aid.startswith("$") and "___" in aid:
            plugin = aid[1:].split("___", 1)[0]
            if plugin not in BUILTIN_PLUGINS:
                plugins_needed.add(plugin)
    if plugins_needed:
        existing_plugins = set(dst_profile.get("additionalNativePluginNames") or [])
        added_plugins = plugins_needed - existing_plugins
        if added_plugins:
            dst_profile.setdefault("additionalNativePluginNames", [])
            dst_profile["additionalNativePluginNames"].extend(sorted(added_plugins))
            print(f"  declared plugins: {sorted(added_plugins)}")
            # Mirror in metadata/AdvancedInfo.json
            adv_path = dst_root / "metadata" / "AdvancedInfo.json"
            adv = {"additionalPluginNames": []}
            if adv_path.is_file():
                with adv_path.open(encoding="utf-8") as f:
                    adv = json.load(f)
            adv.setdefault("additionalPluginNames", [])
            for pl in added_plugins:
                if pl not in adv["additionalPluginNames"]:
                    adv["additionalPluginNames"].append(pl)
            if not dry_run:
                adv_path.parent.mkdir(exist_ok=True)
                with adv_path.open("w", encoding="utf-8") as f:
                    json.dump(adv, f, indent=4, ensure_ascii=False)

    if not dry_run:
        save_profile(dst_root, dst_profile)
        print(f"  saved {dst_root / 'ProfileInfo.json'}")


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--src", required=True, help="source profile dir")
    ap.add_argument("--src-workspace", type=int, default=0, dest="src_workspace")
    ap.add_argument("--src-page", required=True, dest="src_page", help="page index in workspace OR page GUID OR displayName")
    ap.add_argument("--kind", choices=["touch", "encoder", "wheel"], default="touch")
    ap.add_argument("--dst", action="append", required=True, help="destination profile dir (repeatable)")
    ap.add_argument("--dst-workspace", type=int, default=0, dest="dst_workspace")
    ap.add_argument("--strategy", choices=["clone-page", "replace-page"], default="clone-page")
    ap.add_argument("--rename", default=None, help="rename the page in the destination(s)")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    src_root, src_profile = load_profile(args.src)
    src_ws = find_workspace(src_profile, args.src_workspace)
    src_page = find_page(src_profile, args.kind, args.src_page, workspace=src_ws)
    action_ids = _collect_action_ids(src_page.get("controls") or [], kind=args.kind)
    # Wheel pages reference actions via .parameters
    if args.kind == "wheel":
        for v in (src_page.get("parameters") or {}).values():
            if isinstance(v, str) and v.startswith("$"):
                action_ids.add(v)

    print(f"source: {args.src}  {args.kind}-page {src_page['displayName']!r}  ({len(action_ids)} action refs)")
    for dst in args.dst:
        propagate_to(src_root, src_profile, src_page,
                     kind=args.kind, action_ids=action_ids,
                     dst_path=Path(dst), dst_workspace_idx=args.dst_workspace,
                     strategy=args.strategy, rename=args.rename, dry_run=args.dry_run)


if __name__ == "__main__":
    main()
