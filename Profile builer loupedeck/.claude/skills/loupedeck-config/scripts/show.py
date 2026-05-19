"""Pretty-print a profile.

Usage:
    python inspect.py <profile_dir> [--json] [--workspace N] [--show-empty]
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path

from _common import load_profile


def _short_action(a: str | None) -> str:
    if a is None or a == "":
        return "·"
    # collapse very long IDs
    return a if len(a) < 80 else a[:77] + "..."


def summarize(profile: dict, *, only_workspace: int | None, show_empty: bool) -> dict:
    out: dict = {
        "name": profile.get("name"),
        "displayName": profile.get("displayName"),
        "deviceType": profile.get("deviceType"),
        "applicationName": profile.get("applicationName"),
        "additionalNativePluginNames": profile.get("additionalNativePluginNames", []),
        "modes": [],
        "roundPage": _page_text(profile["layout"].get("roundPage"), show_empty),
        "squarePage": _page_text(profile["layout"].get("squarePage"), show_empty),
        "counts": {
            "macroCommands": len(profile.get("macroCommands") or []),
            "macroAdjustments": len(profile.get("macroAdjustments") or []),
            "profileActions": len(profile.get("profileActions") or []),
        },
    }

    for mode in profile["layout"]["layoutModes"]:
        touch_pages = {p["name"]: p for p in mode.get("touchPages") or []}
        encoder_pages = {p["name"]: p for p in mode.get("encoderPages") or []}
        wheel_pages = {p["name"]: p for p in mode.get("wheelPages") or []}
        mode_out = {
            "modeName": mode["modeName"],
            "homeWorkspaceName": mode.get("homeWorkspaceName"),
            "workspaces": [],
        }
        for i, ws in enumerate(mode.get("workspaces") or []):
            if only_workspace is not None and i != only_workspace:
                continue
            ws_out = {
                "index": i,
                "name": ws["name"],
                "displayName": ws["displayName"],
                "touchPages": [],
                "encoderPages": [],
                "wheelPages": [],
            }
            for j, page_name in enumerate(ws.get("touchPageNames") or []):
                page = touch_pages.get(page_name)
                ws_out["touchPages"].append(_summarize_button_page(page, j, show_empty))
            for j, page_name in enumerate(ws.get("encoderPageNames") or []):
                page = encoder_pages.get(page_name)
                ws_out["encoderPages"].append(_summarize_encoder_page(page, j, show_empty))
            for j, page_name in enumerate(ws.get("wheelPageNames") or []):
                page = wheel_pages.get(page_name)
                ws_out["wheelPages"].append({
                    "index": j,
                    "name": page["name"] if page else page_name,
                    "displayName": page["displayName"] if page else "<missing>",
                    "templateName": (page or {}).get("templateName"),
                    "parameters": (page or {}).get("parameters"),
                })
            mode_out["workspaces"].append(ws_out)
        out["modes"].append(mode_out)
    return out


def _summarize_button_page(page: dict | None, index: int, show_empty: bool) -> dict:
    if page is None:
        return {"index": index, "name": "<missing>", "controls": []}
    controls = []
    for k, ctl in enumerate(page.get("controls") or []):
        if not show_empty and ctl.get("pressAction") in (None, "") and ctl.get("fnPressAction") in (None, ""):
            continue
        controls.append({
            "i": k,
            "press": ctl.get("pressAction"),
            "fn": ctl.get("fnPressAction"),
        })
    return {
        "index": index,
        "name": page["name"],
        "displayName": page.get("displayName"),
        "controls": controls,
        "controlCount": len(page.get("controls") or []),
    }


def _summarize_encoder_page(page: dict | None, index: int, show_empty: bool) -> dict:
    if page is None:
        return {"index": index, "name": "<missing>", "controls": []}
    controls = []
    for k, ctl in enumerate(page.get("controls") or []):
        empty = all(ctl.get(f) in (None, "") for f in ("pressAction", "fnPressAction", "rotateAction", "fnRotateAction"))
        if not show_empty and empty:
            continue
        controls.append({
            "i": k,
            "press": ctl.get("pressAction"),
            "rotate": ctl.get("rotateAction"),
            "fnPress": ctl.get("fnPressAction"),
            "fnRotate": ctl.get("fnRotateAction"),
        })
    return {
        "index": index,
        "name": page["name"],
        "displayName": page.get("displayName"),
        "controls": controls,
        "controlCount": len(page.get("controls") or []),
    }


def _page_text(page: dict | None, show_empty: bool) -> dict | None:
    if not page:
        return None
    return _summarize_button_page(page, 0, show_empty)


def _print_text(data: dict) -> None:
    print(f"Profile: {data['displayName']!r}  (name={data['name']}, device={data['deviceType']}, app={data['applicationName']})")
    if data["additionalNativePluginNames"]:
        print(f"  plugins: {', '.join(data['additionalNativePluginNames'])}")
    print(f"  macros: {data['counts']['macroCommands']} commands, "
          f"{data['counts']['macroAdjustments']} adjustments, "
          f"{data['counts']['profileActions']} profile-actions")
    for mode in data["modes"]:
        print(f"\n  Mode {mode['modeName']!r}  (home workspace: {mode['homeWorkspaceName']})")
        for ws in mode["workspaces"]:
            print(f"    Workspace [{ws['index']}] {ws['displayName']!r}  ({ws['name']})")
            for p in ws["touchPages"]:
                print(f"      Touch page [{p['index']}] {p['displayName']!r}  ({p['controlCount']} buttons)")
                for c in p["controls"]:
                    print(f"        b{c['i']:>2}: press={_short_action(c['press'])}"
                          + (f"  fn={_short_action(c['fn'])}" if c.get("fn") else ""))
            for p in ws["encoderPages"]:
                print(f"      Encoder page [{p['index']}] {p['displayName']!r}  ({p['controlCount']} encoders)")
                for c in p["controls"]:
                    bits = []
                    if c.get("rotate"): bits.append(f"rot={_short_action(c['rotate'])}")
                    if c.get("press"): bits.append(f"press={_short_action(c['press'])}")
                    if c.get("fnPress"): bits.append(f"fnPress={_short_action(c['fnPress'])}")
                    if c.get("fnRotate"): bits.append(f"fnRot={_short_action(c['fnRotate'])}")
                    print(f"        e{c['i']:>2}: {'  '.join(bits)}")
            for p in ws["wheelPages"]:
                print(f"      Wheel page [{p['index']}] {p['displayName']!r}  template={p['templateName']}")
    if data["roundPage"]:
        print(f"\n  Round page: {data['roundPage']['controlCount']} buttons")
        for c in data["roundPage"]["controls"]:
            print(f"    r{c['i']:>2}: press={_short_action(c['press'])}")
    if data["squarePage"]:
        print(f"\n  Square page: {data['squarePage']['controlCount']} buttons")
        for c in data["squarePage"]["controls"]:
            print(f"    s{c['i']:>2}: press={_short_action(c['press'])}"
                  + (f"  fn={_short_action(c['fn'])}" if c.get("fn") else ""))


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("profile_dir")
    ap.add_argument("--json", action="store_true", help="emit JSON instead of text")
    ap.add_argument("--workspace", type=int, default=None, help="restrict to a single workspace index")
    ap.add_argument("--show-empty", action="store_true", help="include unbound buttons")
    args = ap.parse_args()

    _, profile = load_profile(args.profile_dir)
    summary = summarize(profile, only_workspace=args.workspace, show_empty=args.show_empty)
    if args.json:
        print(json.dumps(summary, indent=2, ensure_ascii=False))
    else:
        _print_text(summary)


if __name__ == "__main__":
    main()
