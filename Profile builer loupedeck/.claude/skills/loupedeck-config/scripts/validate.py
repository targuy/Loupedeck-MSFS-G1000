"""Cross-check a profile for consistency.

Usage:
    python validate.py <profile_dir>

Exit code 0 = clean, 1 = warnings only, 2 = errors found.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

from _common import load_profile


GUID_RE = re.compile(r"^[A-F0-9]{32}$")
ACTION_REF_RE = re.compile(r"\$@Generic___@(Macro|MacroAdjustment|ProfileAction)___([A-F0-9]{32})")
PLUGIN_RE = re.compile(r"^\$([@A-Za-z0-9_]+)___")

# Built-in plugins that don't need to appear in additionalNativePluginNames.
# Confirmed empirically: a CT default profile uses $DefaultWin___... actions with
# additionalNativePluginNames = []. @Generic is always built-in.
BUILTIN_PLUGINS = {"DefaultWin", "DefaultMac", "DefaultLinux"}


def _check_guid(label: str, guid: str, errors: list, warnings: list) -> None:
    if not GUID_RE.match(guid or ""):
        errors.append(f"{label}: bad GUID format {guid!r} (expected 32 uppercase hex, no dashes)")


def validate(root: Path, profile: dict) -> tuple[list[str], list[str]]:
    errors: list[str] = []
    warnings: list[str] = []

    _check_guid("profile.name", profile.get("name"), errors, warnings)

    declared_plugins = set(profile.get("additionalNativePluginNames") or [])
    adv_path = root / "metadata" / "AdvancedInfo.json"
    adv_plugins: set[str] = set()
    if adv_path.is_file():
        with adv_path.open(encoding="utf-8") as f:
            adv_plugins = set((json.load(f).get("additionalPluginNames") or []))
        diff = (declared_plugins ^ adv_plugins)
        if diff:
            warnings.append(f"plugin lists differ between ProfileInfo.additionalNativePluginNames "
                            f"and metadata/AdvancedInfo.json: only-in-one = {sorted(diff)}")
    else:
        warnings.append("metadata/AdvancedInfo.json missing")

    # collect every page id present
    mode_pages: dict[str, set[str]] = {"touch": set(), "encoder": set(), "wheel": set()}
    for mode in profile["layout"]["layoutModes"]:
        for kind, bucket in (("touch", "touchPages"), ("encoder", "encoderPages"), ("wheel", "wheelPages")):
            for p in mode.get(bucket) or []:
                _check_guid(f"{bucket} '{p.get('displayName')}'", p.get("name"), errors, warnings)
                if p["name"] in mode_pages[kind]:
                    errors.append(f"duplicate {kind} page GUID: {p['name']}")
                mode_pages[kind].add(p["name"])

        # workspace references
        for ws in mode.get("workspaces") or []:
            _check_guid(f"workspace '{ws.get('displayName')}'", ws.get("name"), errors, warnings)
            for kind, ws_key, bucket in (("touch", "touchPageNames", "touchPages"),
                                          ("encoder", "encoderPageNames", "encoderPages"),
                                          ("wheel", "wheelPageNames", "wheelPages")):
                for pn in ws.get(ws_key) or []:
                    if pn not in mode_pages[kind]:
                        errors.append(f"workspace {ws['displayName']!r} references missing {kind} page {pn}")

        home = mode.get("homeWorkspaceName")
        ws_names = {ws["name"] for ws in mode.get("workspaces") or []}
        if home and home not in ws_names:
            errors.append(f"homeWorkspaceName {home} not in workspaces")

    # plugin coverage in action IDs
    needed_plugins: set[str] = set()

    def scan_action(aid: str | None, where: str) -> None:
        if not aid:
            return
        m = PLUGIN_RE.match(aid)
        if not m:
            warnings.append(f"{where}: unrecognised action ID {aid!r}")
            return
        plugin = m.group(1)
        if not plugin.startswith("@"):
            needed_plugins.add(plugin)
        ref = ACTION_REF_RE.match(aid)
        if ref:
            kind, guid = ref.group(1), ref.group(2)
            bucket = {"Macro": "macroCommands", "MacroAdjustment": "macroAdjustments", "ProfileAction": "profileActions"}[kind]
            entries = profile.get(bucket) or []
            if kind == "ProfileAction":
                if not any(e.get("name") == aid for e in entries):
                    errors.append(f"{where}: profileAction {guid} referenced but not defined")
            else:
                if not any(e.get("name") == guid for e in entries):
                    errors.append(f"{where}: {kind.lower()} {guid} referenced but not defined")

    for mode in profile["layout"]["layoutModes"]:
        for bucket, kind in (("touchPages", "touch"), ("encoderPages", "encoder")):
            for p in mode.get(bucket) or []:
                for i, ctl in enumerate(p.get("controls") or []):
                    if kind == "touch":
                        scan_action(ctl.get("pressAction"), f"touch '{p['displayName']}' btn{i}")
                        scan_action(ctl.get("fnPressAction"), f"touch '{p['displayName']}' btn{i} fn")
                    else:
                        for f in ("pressAction", "rotateAction", "fnPressAction", "fnRotateAction"):
                            scan_action(ctl.get(f), f"encoder '{p['displayName']}' enc{i} {f}")
        for p in mode.get("wheelPages") or []:
            for v in (p.get("parameters") or {}).values():
                if isinstance(v, str) and v.startswith("$"):
                    scan_action(v, f"wheel '{p['displayName']}'")
    for kind, bucket in (("round", "roundPage"), ("square", "squarePage")):
        p = profile["layout"].get(bucket)
        if p:
            for i, ctl in enumerate(p.get("controls") or []):
                scan_action(ctl.get("pressAction"), f"{kind} btn{i}")
                scan_action(ctl.get("fnPressAction"), f"{kind} btn{i} fn")

    missing_plugins = needed_plugins - declared_plugins - BUILTIN_PLUGINS
    if missing_plugins:
        errors.append(f"plugins used but not declared in additionalNativePluginNames: {sorted(missing_plugins)}")
    extra_plugins = declared_plugins - needed_plugins
    if extra_plugins:
        warnings.append(f"plugins declared but not referenced anywhere: {sorted(extra_plugins)}")

    return errors, warnings


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("profile_dir")
    args = ap.parse_args()

    root, profile = load_profile(args.profile_dir)
    errors, warnings = validate(root, profile)

    for w in warnings:
        print(f"warning: {w}")
    for e in errors:
        print(f"error: {e}", file=sys.stderr)
    if errors:
        print(f"\n{len(errors)} error(s), {len(warnings)} warning(s). FAIL.")
        sys.exit(2)
    if warnings:
        print(f"\n0 errors, {len(warnings)} warning(s). OK with warnings.")
        sys.exit(1)
    print("\nclean.")


if __name__ == "__main__":
    main()
