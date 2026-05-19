"""Shared helpers for the loupedeck-config skill scripts."""
from __future__ import annotations

import json
import re
import shutil
import sys
import uuid
from pathlib import Path
from typing import Any


PLUGIN_RE = re.compile(r"^\$([@A-Za-z0-9_]+)___")
BUILTIN_PLUGINS = {"DefaultWin", "DefaultMac", "DefaultLinux"}


def new_guid() -> str:
    """Loupedeck-style GUID: 32 uppercase hex chars, no dashes."""
    return uuid.uuid4().hex.upper()


def profile_root(profile_dir: str | Path) -> Path:
    p = Path(profile_dir)
    if not p.is_dir():
        die(f"not a directory: {p}")
    if not (p / "ProfileInfo.json").is_file():
        die(f"no ProfileInfo.json in {p} — did you point at an extracted profile dir?")
    return p


def load_profile(profile_dir: str | Path) -> tuple[Path, dict[str, Any]]:
    root = profile_root(profile_dir)
    with (root / "ProfileInfo.json").open(encoding="utf-8") as f:
        return root, json.load(f)


def save_profile(root: Path, data: dict[str, Any], *, backup: bool = True) -> None:
    sync_plugin_declarations(root, data)
    target = root / "ProfileInfo.json"
    if backup and target.exists():
        shutil.copy2(target, target.with_suffix(".json.bak"))
    with target.open("w", encoding="utf-8") as f:
        json.dump(data, f, indent=4, ensure_ascii=False)
        f.write("\n")
    # Invalidate the preview cache — configui will regenerate it on import/open.
    preview = root / "metadata" / "ProfilePreview.json"
    if preview.exists():
        preview.unlink()


def sync_plugin_declarations(root: Path, profile: dict[str, Any]) -> None:
    """Keep plugin declarations aligned with action IDs used in ProfileInfo.json."""
    needed = set()
    native = profile.get("nativePluginName")

    def visit(value: Any) -> None:
        if isinstance(value, str):
            match = PLUGIN_RE.match(value)
            if match:
                plugin = match.group(1)
                if not plugin.startswith("@") and plugin not in BUILTIN_PLUGINS and plugin != native:
                    needed.add(plugin)
            return
        if isinstance(value, dict):
            for child in value.values():
                visit(child)
            return
        if isinstance(value, list):
            for child in value:
                visit(child)

    visit(profile)

    existing = list(profile.get("additionalNativePluginNames") or [])
    merged = existing[:]
    for plugin in sorted(needed):
        if plugin not in merged:
            merged.append(plugin)
    profile["additionalNativePluginNames"] = merged

    metadata = root / "metadata"
    metadata.mkdir(exist_ok=True)
    adv_path = metadata / "AdvancedInfo.json"
    adv = {}
    if adv_path.is_file():
        with adv_path.open(encoding="utf-8") as f:
            adv = json.load(f)
    adv_plugins = list(adv.get("additionalPluginNames") or [])
    for plugin in merged:
        if plugin not in adv_plugins:
            adv_plugins.append(plugin)
    adv["additionalPluginNames"] = adv_plugins
    with adv_path.open("w", encoding="utf-8") as f:
        json.dump(adv, f, indent=4, ensure_ascii=False)
        f.write("\n")


def die(msg: str, code: int = 2) -> None:
    print(f"error: {msg}", file=sys.stderr)
    sys.exit(code)


def find_workspace(profile: dict, ws_index_or_id: str | int) -> dict:
    modes = profile["layout"]["layoutModes"]
    if len(modes) != 1:
        # Most profiles have a single mode; if there are more, require explicit naming.
        names = [m["modeName"] for m in modes]
        die(f"profile has multiple modes {names}; specify --mode")
    mode = modes[0]
    workspaces = mode["workspaces"]
    if isinstance(ws_index_or_id, int) or str(ws_index_or_id).isdigit():
        idx = int(ws_index_or_id)
        if idx < 0 or idx >= len(workspaces):
            die(f"workspace index {idx} out of range [0..{len(workspaces) - 1}]")
        return workspaces[idx]
    for ws in workspaces:
        if ws["name"] == ws_index_or_id or ws["displayName"] == ws_index_or_id:
            return ws
    die(f"workspace not found: {ws_index_or_id}")


def find_mode(profile: dict) -> dict:
    modes = profile["layout"]["layoutModes"]
    if len(modes) == 1:
        return modes[0]
    die(f"profile has {len(modes)} modes; multi-mode editing not supported by this script")


def find_page(profile: dict, kind: str, name_or_idx: str | int, *, workspace: dict | None = None) -> dict:
    """kind in {'touch','encoder','wheel'}. workspace optional: if provided, restrict to its page list and index within it."""
    mode = find_mode(profile)
    bucket_key = {"touch": "touchPages", "encoder": "encoderPages", "wheel": "wheelPages"}[kind]
    ws_key = {"touch": "touchPageNames", "encoder": "encoderPageNames", "wheel": "wheelPageNames"}[kind]
    all_pages = {p["name"]: p for p in mode[bucket_key]}
    if workspace is not None:
        page_names = workspace[ws_key]
        if str(name_or_idx).isdigit():
            idx = int(name_or_idx)
            if idx < 0 or idx >= len(page_names):
                die(f"{kind} page index {idx} out of range [0..{len(page_names) - 1}] for workspace {workspace['displayName']}")
            return all_pages[page_names[idx]]
        if name_or_idx in all_pages and name_or_idx in page_names:
            return all_pages[name_or_idx]
        for n in page_names:
            if all_pages[n]["displayName"] == name_or_idx:
                return all_pages[n]
        die(f"{kind} page not found in workspace: {name_or_idx}")
    # global lookup
    if name_or_idx in all_pages:
        return all_pages[name_or_idx]
    for p in mode[bucket_key]:
        if p["displayName"] == name_or_idx:
            return p
    die(f"{kind} page not found: {name_or_idx}")


def make_touch_button() -> dict:
    return {
        "$type": "Loupedeck.Service.ProfileLayoutButton, LoupedeckService",
        "pressAction": None,
        "fnPressAction": None,
    }


def make_encoder() -> dict:
    return {
        "$type": "Loupedeck.Service.ProfileLayoutEncoder, LoupedeckService",
        "pressAction": None,
        "fnPressAction": None,
        "rotateAction": None,
        "fnRotateAction": None,
    }


def diff_action(before: Any, after: Any) -> str:
    if before == after:
        return f"= {before!r}"
    return f"- {before!r}\n+ {after!r}"
