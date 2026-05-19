"""Mutate a profile: set buttons/encoders, add/delete pages and workspaces.

Usage examples:
    python edit.py set-button   <dir> --workspace 0 --page 1 --index 4 --action '$DefaultWin___Volume'
    python edit.py set-encoder  <dir> --workspace 0 --page 0 --index 2 --press '...' --rotate '...'
    python edit.py set-round    <dir> --index 0 --action '...'
    python edit.py set-square   <dir> --index 6 --action '...' --fn-action '...'
    python edit.py add-page     <dir> --workspace 0 --kind touch --name "My new page"
    python edit.py delete-page  <dir> --page-id <GUID>
    python edit.py add-workspace <dir> --name "Custom workspace"

All commands accept --dry-run. Writes back up ProfileInfo.json to ProfileInfo.json.bak.
"""
from __future__ import annotations

import argparse
import copy
import json
from pathlib import Path

from _common import (
    die, find_mode, find_page, find_workspace,
    load_profile, make_encoder, make_touch_button, new_guid, save_profile,
)


def _print_change(label: str, before, after, dry_run: bool) -> None:
    arrow = "[dry-run]" if dry_run else "[write]"
    print(f"{arrow} {label}\n  before: {before!r}\n  after:  {after!r}")


def cmd_set_button(args, profile: dict) -> None:
    ws = find_workspace(profile, args.workspace)
    page = find_page(profile, "touch", args.page, workspace=ws)
    controls = page["controls"]
    if args.index < 0 or args.index >= len(controls):
        die(f"button index {args.index} out of range [0..{len(controls) - 1}] for page {page['displayName']!r}")
    ctl = controls[args.index]
    before = (ctl.get("pressAction"), ctl.get("fnPressAction"))
    if args.action is not None:
        ctl["pressAction"] = args.action if args.action != "" else None
    if args.fn_action is not None:
        ctl["fnPressAction"] = args.fn_action if args.fn_action != "" else None
    after = (ctl.get("pressAction"), ctl.get("fnPressAction"))
    _print_change(f"workspace[{args.workspace}] touchPage[{args.page}] button[{args.index}]", before, after, args.dry_run)


def cmd_set_encoder(args, profile: dict) -> None:
    ws = find_workspace(profile, args.workspace)
    page = find_page(profile, "encoder", args.page, workspace=ws)
    controls = page["controls"]
    if args.index < 0 or args.index >= len(controls):
        die(f"encoder index {args.index} out of range [0..{len(controls) - 1}]")
    ctl = controls[args.index]
    before = {k: ctl.get(k) for k in ("pressAction", "rotateAction", "fnPressAction", "fnRotateAction")}
    for arg_name, key in (("press", "pressAction"), ("rotate", "rotateAction"),
                          ("fn_press", "fnPressAction"), ("fn_rotate", "fnRotateAction")):
        val = getattr(args, arg_name)
        if val is not None:
            ctl[key] = val if val != "" else None
    after = {k: ctl.get(k) for k in ("pressAction", "rotateAction", "fnPressAction", "fnRotateAction")}
    _print_change(f"workspace[{args.workspace}] encoderPage[{args.page}] encoder[{args.index}]", before, after, args.dry_run)


def cmd_set_round(args, profile: dict) -> None:
    page = profile["layout"].get("roundPage")
    if not page:
        die("this profile has no roundPage (older device?)")
    controls = page["controls"]
    if args.index < 0 or args.index >= len(controls):
        die(f"round index {args.index} out of range [0..{len(controls) - 1}]")
    ctl = controls[args.index]
    before = ctl.get("pressAction")
    ctl["pressAction"] = args.action if args.action != "" else None
    _print_change(f"roundPage button[{args.index}]", before, ctl.get("pressAction"), args.dry_run)


def cmd_set_square(args, profile: dict) -> None:
    page = profile["layout"].get("squarePage")
    if not page:
        die("this profile has no squarePage")
    controls = page["controls"]
    if args.index < 0 or args.index >= len(controls):
        die(f"square index {args.index} out of range [0..{len(controls) - 1}]")
    ctl = controls[args.index]
    before = (ctl.get("pressAction"), ctl.get("fnPressAction"))
    if args.action is not None:
        ctl["pressAction"] = args.action if args.action != "" else None
    if args.fn_action is not None:
        ctl["fnPressAction"] = args.fn_action if args.fn_action != "" else None
    after = (ctl.get("pressAction"), ctl.get("fnPressAction"))
    _print_change(f"squarePage button[{args.index}]", before, after, args.dry_run)


def cmd_add_page(args, profile: dict) -> None:
    mode = find_mode(profile)
    ws = find_workspace(profile, args.workspace)
    new_id = new_guid()
    if args.kind == "touch":
        # Infer control count from the workspace's first existing touch page if any, else 15.
        existing_names = ws.get("touchPageNames") or []
        if existing_names:
            template = next(p for p in mode["touchPages"] if p["name"] == existing_names[0])
            count = len(template["controls"])
        else:
            count = 15
        page = {
            "$type": "Loupedeck.Service.ProfileLayoutButtonPage, LoupedeckService",
            "name": new_id,
            "displayName": args.name,
            "description": None,
            "controls": [make_touch_button() for _ in range(count)],
            "dynamicPageName": None,
            "dynamicPagePluginName": None,
            "dynamicPageNumber": 0,
        }
        mode["touchPages"].append(page)
        ws["touchPageNames"].append(new_id)
    elif args.kind == "encoder":
        existing_names = ws.get("encoderPageNames") or []
        if existing_names:
            template = next(p for p in mode["encoderPages"] if p["name"] == existing_names[0])
            count = len(template["controls"])
        else:
            count = 6
        page = {
            "$type": "Loupedeck.Service.ProfileLayoutEncoderPage, LoupedeckService",
            "name": new_id,
            "displayName": args.name,
            "description": None,
            "controls": [make_encoder() for _ in range(count)],
            "dynamicPageName": None,
            "dynamicPagePluginName": None,
            "dynamicPageNumber": 0,
        }
        mode["encoderPages"].append(page)
        ws["encoderPageNames"].append(new_id)
    elif args.kind == "wheel":
        page = {
            "$type": "Loupedeck.Service.ProfileLayoutWheelPage, LoupedeckService",
            "name": new_id,
            "displayName": args.name,
            "description": None,
            "templateName": "WheelToolAnalogClock",
            "parameters": {
                "$type": "Loupedeck.StringDictionaryNoCase, PluginApi",
                "actions": "$@Generic___@ButtonClock",
                "adjustment": "$@Generic___@MouseWheel",
            },
        }
        mode["wheelPages"].append(page)
        ws["wheelPageNames"].append(new_id)
    else:
        die(f"unknown page kind: {args.kind}")
    _print_change(f"add {args.kind} page {args.name!r} to workspace {ws['displayName']!r}", None, new_id, args.dry_run)


def cmd_delete_page(args, profile: dict) -> None:
    mode = find_mode(profile)
    found = False
    for bucket_key, ws_key in (("touchPages", "touchPageNames"),
                               ("encoderPages", "encoderPageNames"),
                               ("wheelPages", "wheelPageNames")):
        bucket = mode.get(bucket_key) or []
        for p in list(bucket):
            if p["name"] == args.page_id:
                bucket.remove(p)
                found = True
                break
    if not found:
        die(f"no page with id {args.page_id} found")
    # Clean references in all workspaces of this mode.
    for ws in mode.get("workspaces") or []:
        for ws_key in ("touchPageNames", "encoderPageNames", "wheelPageNames"):
            if args.page_id in (ws.get(ws_key) or []):
                ws[ws_key] = [n for n in ws[ws_key] if n != args.page_id]
    # Clean nav-actions in remaining pages that reference this page (ChangeTouchPage/ChangeEncoderPage).
    needle = f"|{args.page_id}"
    cleaned = 0
    for bucket_key in ("touchPages", "encoderPages"):
        for p in mode.get(bucket_key) or []:
            for ctl in p.get("controls") or []:
                for k in ("pressAction", "fnPressAction"):
                    if ctl.get(k) and needle in ctl[k]:
                        ctl[k] = None
                        cleaned += 1
    print(f"  cleaned {cleaned} nav-action references")
    _print_change(f"delete page {args.page_id}", "(present)", "(removed)", args.dry_run)


def cmd_add_workspace(args, profile: dict) -> None:
    mode = find_mode(profile)
    ws = {
        "$type": "Loupedeck.Service.ProfileLayoutWorkspace20, LoupedeckService",
        "name": new_guid(),
        "displayName": args.name,
        "description": None,
        "touchPageNames": [],
        "encoderPageNames": [],
        "wheelPageNames": [],
        "activationActions": [],
    }
    mode["workspaces"].append(ws)
    _print_change(f"add workspace {args.name!r}", None, ws["name"], args.dry_run)


COMMANDS = {
    "set-button": cmd_set_button,
    "set-encoder": cmd_set_encoder,
    "set-round": cmd_set_round,
    "set-square": cmd_set_square,
    "add-page": cmd_add_page,
    "delete-page": cmd_delete_page,
    "add-workspace": cmd_add_workspace,
}


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)

    common_dir = lambda p: p.add_argument("profile_dir")
    common_dry = lambda p: p.add_argument("--dry-run", action="store_true")

    p = sub.add_parser("set-button")
    common_dir(p); common_dry(p)
    p.add_argument("--workspace", type=int, default=0)
    p.add_argument("--page", type=int, required=True)
    p.add_argument("--index", type=int, required=True)
    p.add_argument("--action", default=None, help="press action ID (use '' to clear)")
    p.add_argument("--fn-action", default=None, dest="fn_action", help="fn press action ID")

    p = sub.add_parser("set-encoder")
    common_dir(p); common_dry(p)
    p.add_argument("--workspace", type=int, default=0)
    p.add_argument("--page", type=int, required=True)
    p.add_argument("--index", type=int, required=True)
    p.add_argument("--press", default=None)
    p.add_argument("--rotate", default=None)
    p.add_argument("--fn-press", default=None, dest="fn_press")
    p.add_argument("--fn-rotate", default=None, dest="fn_rotate")

    p = sub.add_parser("set-round")
    common_dir(p); common_dry(p)
    p.add_argument("--index", type=int, required=True)
    p.add_argument("--action", required=True)

    p = sub.add_parser("set-square")
    common_dir(p); common_dry(p)
    p.add_argument("--index", type=int, required=True)
    p.add_argument("--action", default=None)
    p.add_argument("--fn-action", default=None, dest="fn_action")

    p = sub.add_parser("add-page")
    common_dir(p); common_dry(p)
    p.add_argument("--workspace", type=int, default=0)
    p.add_argument("--kind", choices=["touch", "encoder", "wheel"], required=True)
    p.add_argument("--name", required=True)

    p = sub.add_parser("delete-page")
    common_dir(p); common_dry(p)
    p.add_argument("--page-id", required=True, dest="page_id")

    p = sub.add_parser("add-workspace")
    common_dir(p); common_dry(p)
    p.add_argument("--name", required=True)

    args = ap.parse_args()
    root, profile = load_profile(args.profile_dir)
    COMMANDS[args.cmd](args, profile)
    if not args.dry_run:
        save_profile(root, profile)
        print(f"  saved {root / 'ProfileInfo.json'} (backup at .json.bak)")


if __name__ == "__main__":
    main()
