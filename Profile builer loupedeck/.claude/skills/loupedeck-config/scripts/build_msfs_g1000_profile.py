"""Build baseline MSFS G1000 pages in an extracted Loupedeck CT profile.

Usage:
    python build_msfs_g1000_profile.py <profile_dir> [--workspace 0] [--pack out.lp5]

The script edits a copied or extracted profile directory. It creates or replaces
named touch/encoder pages and binds them to the concrete LoupedeckMSFSG1000
plugin actions.
"""
from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path

from _common import find_workspace, load_profile, make_encoder, make_touch_button, new_guid, save_profile


PLUGIN = "LoupedeckMSFSG1000"


def aid(class_name: str) -> str:
    return f"${PLUGIN}___LoupedeckMSFSG1000.Actions.{class_name}"


ACTIONS = {
    "ap_master": aid("AutopilotMasterCommand"),
    "ap_hdg": aid("MsfsApHdgCommand"),
    "ap_nav": aid("MsfsApNavCommand"),
    "ap_alt": aid("MsfsApAltCommand"),
    "ap_vs": aid("MsfsApVsCommand"),
    "ap_apr": aid("MsfsApAprCommand"),
    "ap_flc": aid("MsfsApFlcCommand"),
    "gear": aid("MsfsGearToggleCommand"),
    "parking": aid("MsfsParkingBrakeCommand"),
    "flaps_up": aid("MsfsFlapsUpCommand"),
    "flaps_down": aid("MsfsFlapsDownCommand"),
    "pause": aid("MsfsPauseCommand"),
    "atc": aid("MsfsAtcPanelCommand"),
    "vfr_map": aid("MsfsVfrMapCommand"),
    "status": aid("G1000StatusCommand"),
    "battery": aid("MsfsBatteryMasterCommand"),
    "avionics": aid("MsfsAvionicsMasterCommand"),
    "fuel_pump": aid("MsfsFuelPumpCommand"),
    "magnetos": aid("MsfsMagnetosBothCommand"),
    "starter1": aid("MsfsStarter1Command"),
    "lights_nav": aid("MsfsNavLightsCommand"),
    "lights_beacon": aid("MsfsBeaconCommand"),
    "lights_strobe": aid("MsfsStrobeCommand"),
    "lights_landing": aid("MsfsLandingLightsCommand"),
    "lights_taxi": aid("MsfsTaxiLightsCommand"),
    "direct_to": aid("G1000DirectToCommand"),
    "menu": aid("G1000MenuCommand"),
    "fpl": aid("G1000FplCommand"),
    "proc": aid("G1000ProcCommand"),
    "clr": aid("G1000ClrCommand"),
    "ent": aid("G1000EntCommand"),
    "com1_swap": aid("G1000Com1SwapCommand"),
    "com2_swap": aid("G1000Com2SwapCommand"),
    "nav1_swap": aid("G1000Nav1SwapCommand"),
    "nav2_swap": aid("G1000Nav2SwapCommand"),
    "hdg_bug": aid("MsfsHeadingBugAdjustment"),
    "alt_select": aid("MsfsAltitudeSelectAdjustment"),
    "vs_select": aid("MsfsVerticalSpeedAdjustment"),
    "elev_trim": aid("MsfsElevatorTrimAdjustment"),
    "flaps": aid("MsfsFlapsAdjustment"),
    "baro": aid("G1000BaroAdjustment"),
    "com1_mhz": aid("G1000Com1MhzAdjustment"),
    "com1_khz": aid("G1000Com1KhzAdjustment"),
    "nav1_mhz": aid("G1000Nav1MhzAdjustment"),
    "nav1_khz": aid("G1000Nav1KhzAdjustment"),
    "g1000_encoder": aid("G1000MappedAdjustment"),
    "msfs_encoder": aid("MsfsMappedAdjustment"),
}

for i in range(1, 13):
    ACTIONS[f"softkey_{i}"] = aid(f"G1000Softkey{i}Command")


TOUCH_PAGES = [
    (
        "Garmin PFD",
        {
            0: "softkey_1",
            1: "softkey_2",
            2: "softkey_3",
            3: "softkey_4",
            4: "softkey_5",
            5: "softkey_6",
            6: "softkey_7",
            7: "softkey_8",
            8: "softkey_9",
            9: "softkey_10",
            10: "softkey_11",
            11: "softkey_12",
            12: "page:Garmin Fixed",
            13: "page:Garmin COM/NAV",
            14: "page:Garmin MFD",
        },
    ),
    (
        "Garmin MFD",
        {
            0: "direct_to",
            1: "fpl",
            2: "proc",
            3: "menu",
            4: "clr",
            5: "ent",
            6: "page:Garmin PFD",
            7: "page:Garmin Fixed",
            8: "page:Garmin COM/NAV",
            9: "page:Garmin AP",
            10: "page:Garmin AP Enc",
            11: "page:Garmin COM/NAV Enc",
            12: "softkey_1",
            13: "fpl",
            14: "direct_to",
        },
    ),
    (
        "Garmin AP",
        {
            0: "ap_master",
            1: "ap_hdg",
            2: "ap_nav",
            3: "ap_alt",
            4: "ap_vs",
            5: "ap_apr",
            6: "ap_flc",
            7: "page:Garmin AP Enc",
            8: "page:Garmin PFD",
            9: "page:Garmin MFD",
            10: "direct_to",
            11: "fpl",
            12: "page:Garmin Fixed",
            13: "page:Garmin COM/NAV",
            14: "page:General Controls",
        },
    ),
    (
        "Garmin COM/NAV",
        {
            0: "com1_swap",
            1: "com2_swap",
            2: "nav1_swap",
            3: "nav2_swap",
            4: "page:Garmin COM/NAV Enc",
            5: "page:Garmin Fixed",
            6: "direct_to",
            7: "fpl",
            8: "proc",
            9: "clr",
            10: "ent",
            11: "menu",
            12: "page:Garmin PFD",
            13: "page:Garmin MFD",
            14: "page:General NAV/COM",
        },
    ),
    (
        "Garmin Fixed",
        {
            0: "direct_to",
            1: "fpl",
            2: "proc",
            3: "menu",
            4: "clr",
            5: "ent",
            6: "softkey_1",
            7: "softkey_12",
            8: "com1_swap",
            9: "nav1_swap",
            10: "com2_swap",
            11: "nav2_swap",
            12: "page:Garmin PFD",
            13: "page:Garmin MFD",
            14: "page:Garmin COM/NAV",
        },
    ),
    (
        "General NAV/COM",
        {
            0: "atc",
            1: "vfr_map",
            2: "page:General Sim",
            3: "page:General Controls",
            4: "page:General Power",
            5: "page:Garmin COM/NAV",
            12: "page:Garmin PFD",
            13: "page:General Controls",
            14: "page:General Sim",
        },
    ),
    (
        "General Controls",
        {
            0: "gear",
            1: "parking",
            2: "flaps_up",
            3: "flaps_down",
            4: "page:General Controls Enc",
            5: "lights_landing",
            6: "lights_taxi",
            7: "lights_nav",
            8: "lights_beacon",
            9: "lights_strobe",
            10: "page:General Power",
            11: "page:General Sim",
            12: "page:Garmin AP",
            13: "page:General NAV/COM",
            14: "page:Garmin PFD",
        },
    ),
    (
        "General Power",
        {
            0: "battery",
            1: "avionics",
            2: "fuel_pump",
            3: "magnetos",
            4: "starter1",
            5: "lights_nav",
            6: "lights_beacon",
            7: "lights_strobe",
            8: "lights_landing",
            9: "lights_taxi",
            10: "parking",
            11: "page:General Controls",
            12: "page:General NAV/COM",
            13: "page:General Sim",
            14: "page:Garmin PFD",
        },
    ),
    (
        "General Sim",
        {
            0: "pause",
            1: "atc",
            2: "vfr_map",
            3: "page:General NAV/COM",
            4: "page:General Controls",
            5: "page:General Power",
            6: "status",
            12: "page:Garmin PFD",
            13: "page:Garmin MFD",
            14: "page:Garmin AP",
        },
    ),
]


ENCODER_PAGES = [
    (
        "Garmin AP Enc",
        {
            0: "hdg_bug",
            1: "alt_select",
            2: "vs_select",
            3: "baro",
            4: "elev_trim",
            5: "flaps",
        },
    ),
    (
        "Garmin COM/NAV Enc",
        {
            0: "com1_mhz",
            1: "com1_khz",
            2: "nav1_mhz",
            3: "nav1_khz",
            4: "g1000_encoder",
            5: "msfs_encoder",
        },
    ),
    (
        "General Controls Enc",
        {
            0: "elev_trim",
            1: "flaps",
            2: "baro",
            3: "hdg_bug",
            4: "alt_select",
            5: "vs_select",
        },
    ),
]


PAGE_ALIASES = {
    "Garmin PFD": ["MENU", "SOFTKEYS", "G1000 PFD", "PFD 1", "PFD 2", "Touch Page (1)"],
    "Garmin MFD": ["GARMIN", "G1000 Nav", "NAV"],
    "Garmin AP": ["AP", "G1000 AP"],
    "Garmin COM/NAV": ["RADIO"],
    "Garmin Fixed": ["KEYPAD"],
    "General Controls": ["LIGHT", "G1000 Lights"],
    "General Power": ["POWER"],
    "Garmin AP Enc": ["AP Enc", "G1000 AP Enc", "Dial Page (1)"],
    "Garmin COM/NAV Enc": ["COM Enc", "G1000 Radio Enc"],
    "General Controls Enc": ["FLT Enc"],
}


def replace_or_create_page(mode: dict, ws: dict, kind: str, name: str, count: int) -> dict:
    bucket_key = {"touch": "touchPages", "encoder": "encoderPages"}[kind]
    ws_key = {"touch": "touchPageNames", "encoder": "encoderPageNames"}[kind]
    names = {name, *PAGE_ALIASES.get(name, [])}
    existing = next((p for p in mode[bucket_key] if p.get("displayName") in names), None)
    controls = [make_touch_button() for _ in range(count)] if kind == "touch" else [make_encoder() for _ in range(count)]
    if existing is not None:
        existing["displayName"] = name
        existing["controls"] = controls
        return existing

    page_type = "ButtonPage" if kind == "touch" else "EncoderPage"
    page = {
        "$type": f"Loupedeck.Service.ProfileLayout{page_type}, LoupedeckService",
        "name": new_guid(),
        "displayName": name,
        "description": None,
        "controls": controls,
        "dynamicPageName": None,
        "dynamicPagePluginName": None,
        "dynamicPageNumber": 0,
    }
    mode[bucket_key].append(page)
    ws[ws_key].append(page["name"])
    return page


def infer_count(mode: dict, ws: dict, kind: str, fallback: int) -> int:
    bucket_key = {"touch": "touchPages", "encoder": "encoderPages"}[kind]
    ws_key = {"touch": "touchPageNames", "encoder": "encoderPageNames"}[kind]
    names = ws.get(ws_key) or []
    if not names:
        return fallback
    first = next((p for p in mode[bucket_key] if p["name"] == names[0]), None)
    return len(first["controls"]) if first else fallback


def apply_pages(profile_dir: Path, workspace_idx: int) -> None:
    root, profile = load_profile(profile_dir)
    mode = profile["layout"]["layoutModes"][0]
    ws = find_workspace(profile, workspace_idx)
    touch_count = infer_count(mode, ws, "touch", 15)
    encoder_count = infer_count(mode, ws, "encoder", 6)

    for page_name, bindings in TOUCH_PAGES:
        replace_or_create_page(mode, ws, "touch", page_name, touch_count)

    for page_name, bindings in ENCODER_PAGES:
        replace_or_create_page(mode, ws, "encoder", page_name, encoder_count)

    reorder_workspace_pages(profile, mode, ws)
    bind_page_controls(profile, mode, ws, touch_count, encoder_count)
    bind_round_page(profile, mode, ws)
    write_page_switch_icons(root, profile)
    save_profile(root, profile)
    print(f"saved {root / 'ProfileInfo.json'}")
    print(f"created/replaced {len(TOUCH_PAGES)} touch pages and {len(ENCODER_PAGES)} encoder pages")
    print("bound round physical buttons to Garmin and General universe entry pages")


def reorder_workspace_pages(profile: dict, mode: dict, ws: dict) -> None:
    touch_by_name = {p["displayName"]: p["name"] for p in mode["touchPages"]}
    encoder_by_name = {p["displayName"]: p["name"] for p in mode["encoderPages"]}

    ordered_touch = [touch_by_name[name] for name, _ in TOUCH_PAGES if name in touch_by_name]
    ordered_encoder = [encoder_by_name[name] for name, _ in ENCODER_PAGES if name in encoder_by_name]
    ws["touchPageNames"] = ordered_touch
    ws["encoderPageNames"] = ordered_encoder
    mode["touchPages"] = [page for page in mode["touchPages"] if page.get("name") in set(ordered_touch)]
    mode["encoderPages"] = [page for page in mode["encoderPages"] if page.get("name") in set(ordered_encoder)]
    prune_page_switch_macros(profile)


def prune_page_switch_macros(profile: dict) -> None:
    expected_names = {name for name, _ in TOUCH_PAGES} | {name for name, _ in ENCODER_PAGES}
    keep = []
    for macro in profile.get("macroCommands") or []:
        display_name = macro.get("displayName") or ""
        if display_name.startswith("MSFS Page "):
            if display_name.removeprefix("MSFS Page ") in expected_names:
                keep.append(macro)
            continue
        if display_name.startswith("G1000 Page "):
            if display_name.removeprefix("G1000 Page ") in expected_names:
                macro["displayName"] = f"MSFS Page {display_name.removeprefix('G1000 Page ')}"
                macro["groupName"] = "MSFS Pages"
                keep.append(macro)
            continue
        keep.append(macro)
    profile["macroCommands"] = keep


def bind_page_controls(profile: dict, mode: dict, ws: dict, touch_count: int, encoder_count: int) -> None:
    touch_pages = {p["displayName"]: p for p in mode["touchPages"]}
    encoder_pages = {p["displayName"]: p for p in mode["encoderPages"]}

    for page_name, bindings in TOUCH_PAGES:
        page = touch_pages[page_name]
        for index, key in bindings.items():
            if index < touch_count:
                page["controls"][index]["pressAction"] = resolve_touch_action(profile, mode, ws, key)

    for page_name, bindings in ENCODER_PAGES:
        page = encoder_pages[page_name]
        for index, key in bindings.items():
            if index < encoder_count:
                page["controls"][index]["rotateAction"] = ACTIONS[key]


def resolve_touch_action(profile: dict, mode: dict, ws: dict, key: str) -> str:
    if not key.startswith("page:"):
        return ACTIONS[key]

    page_name = key.removeprefix("page:")
    touch_pages = {p["displayName"]: p["name"] for p in mode["touchPages"]}
    encoder_pages = {p["displayName"]: p["name"] for p in mode["encoderPages"]}
    if page_name in touch_pages:
        action = "ChangeTouchPage"
        page_id = touch_pages[page_name]
    else:
        action = "ChangeEncoderPage"
        page_id = encoder_pages[page_name]

    change_action = f"$@Generic___@{action}___{mode['modeName']}|{ws['name']}|{page_id}"
    return ensure_page_switch_macro(profile, page_name, change_action)


def bind_round_page(profile: dict, mode: dict, ws: dict) -> None:
    round_page = profile["layout"].get("roundPage")
    if not round_page:
        return

    controls = round_page.get("controls") or []
    if len(controls) < 8:
        return

    mode_name = mode["modeName"]
    workspace_name = ws["name"]
    touch_pages = {p["displayName"]: p["name"] for p in mode["touchPages"]}
    encoder_pages = {p["displayName"]: p["name"] for p in mode["encoderPages"]}
    bindings = [
        ("touch", "Garmin PFD"),
        ("touch", "Garmin MFD"),
        ("touch", "Garmin AP"),
        ("touch", "Garmin COM/NAV"),
        ("touch", "General NAV/COM"),
        ("touch", "General Controls"),
        ("touch", "General Power"),
        ("touch", "General Sim"),
    ]
    for index, (kind, page_name) in enumerate(bindings):
        page_id = (touch_pages if kind == "touch" else encoder_pages).get(page_name)
        if not page_id:
            continue
        action = "ChangeTouchPage" if kind == "touch" else "ChangeEncoderPage"
        change_action = f"$@Generic___@{action}___{mode_name}|{workspace_name}|{page_id}"
        controls[index]["pressAction"] = ensure_page_switch_macro(profile, page_name, change_action)


def ensure_page_switch_macro(profile: dict, display_name: str, change_action: str) -> str:
    macro_name = f"MSFS Page {display_name}"
    macros = profile.setdefault("macroCommands", [])
    for macro in macros:
        if macro.get("displayName") in {macro_name, f"G1000 Page {display_name}"}:
            macro["displayName"] = macro_name
            macro["groupName"] = "MSFS Pages"
            macro["actions"] = [change_action]
            return f"$@Generic___@Macro___{macro['name']}"

    macro_id = new_guid()
    macros.append({
        "$type": "Loupedeck.Service.ApplicationProfileMacroCommand, LoupedeckService",
        "isCommand": True,
        "name": macro_id,
        "displayName": macro_name,
        "description": "",
        "groupName": "MSFS Pages",
        "superGroupName": "@macro",
        "supportedOs": "All",
        "supportedModes": ["system"],
        "showAsSingleAction": True,
        "actionEditorCommands": [],
        "isMultiState": False,
        "actions": [change_action],
    })
    return f"$@Generic___@Macro___{macro_id}"


def write_page_switch_icons(root: Path, profile: dict) -> None:
    icon_dir = root / "ActionIcons"
    icon_dir.mkdir(exist_ok=True)
    for macro in profile.get("macroCommands") or []:
        display_name = macro.get("displayName") or ""
        if display_name.startswith("MSFS Page "):
            label = display_name.removeprefix("MSFS Page ")
        elif display_name.startswith("G1000 Page "):
            label = display_name.removeprefix("G1000 Page ")
        else:
            continue
        action_id = f"$@Generic___@Macro___{macro['name']}"
        icon = {
            "backgroundColor": 4278190080,
            "items": [
                {
                    "text": frame_title(label),
                    "originalText": frame_title(label),
                    "textColor": 4294967295,
                    "fontSize": 9,
                    "fontName": "Arial",
                    "isVisible": True,
                    "itemType": "Text",
                    "area": {
                        "x": 0,
                        "y": 22,
                        "width": 100,
                        "height": 56,
                    },
                },
            ],
        }
        with (icon_dir / f"{action_id}.ict").open("w", encoding="utf-8") as f:
            json.dump(icon, f, indent=4, ensure_ascii=False)
            f.write("\n")


def frame_title(title: str) -> str:
    return f"-- {title} --"


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("profile_dir")
    parser.add_argument("--workspace", type=int, default=0)
    parser.add_argument("--pack", default=None, help="optional .lp5 output path")
    args = parser.parse_args()

    profile_dir = Path(args.profile_dir)
    apply_pages(profile_dir, args.workspace)
    if args.pack:
        lp5_script = Path(__file__).with_name("lp5.py")
        subprocess.check_call([sys.executable, str(lp5_script), "pack", str(profile_dir), args.pack])


if __name__ == "__main__":
    main()
