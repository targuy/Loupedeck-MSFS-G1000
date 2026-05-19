---
name: loupedeck-config
description: Read, edit, validate, and repack Loupedeck (Logi) device profiles. Use when the user wants to inspect a .lp5 export, modify buttons/knobs/pages/macros/profile-actions, generate a new profile, or work with the live runtime profile store under %LOCALAPPDATA%\Logi\LogiPluginService.
---

# loupedeck-config

Tools to manage configuration profiles for **Loupedeck** controllers (now Logi). A profile maps every physical control (touch screen buttons, encoders/dials, round/square buttons, wheel) to an action exposed by a plugin, organised into pages and workspaces.

## What a profile is

A **`.lp5` file is a ZIP archive** with this layout (same layout exists, unzipped, in the live runtime store):

```
ApplicationInfo.json         # which app this profile targets (spad, default, ...)
ApplicationIcon.png          # optional, app icon
ProfileInfo.json             # the profile itself (the big one — read reference/schema.md)
ActionColors/
  ActionColors.json          # composite-key → RGB int per nav-action+page+workspace
ActionIcons/
  $<plugin>___<id>.ict       # JSON: icon background + image + text overlays
ActionImages/
  $<plugin>___<id>.png       # simpler PNG button images
metadata/
  LoupedeckPackage.yaml      # type/name/displayName/version
  AdvancedInfo.json          # additionalPluginNames[]
  ProfilePreview.json        # rendered preview cache (PNG blobs) — invalidate on edit
```

- **Live runtime store**: `%LOCALAPPDATA%\Logi\LogiPluginService\Applications\<DeviceType>\<appName>\Profiles\<profileGUID>\` — same files, unzipped.
- Backups: `Applications.Backups\` sibling directory.

For schema details see `reference/schema.md`. For the action-id grammar see `reference/action-ids.md`. For per-device button counts see `reference/devices.md`.

## When to use this skill

Primary workflows (in priority order):

1. **Edit a profile without clicking through the configui.** Inspect, set buttons/encoders, add pages, edit macros.
2. **Cross-profile sync** — copy a page (e.g. "Sound control", "Windows shortcuts") from one profile into many, so the user doesn't have to switch profiles on the device. See `scripts/propagate.py`.
3. Build a fresh profile from a template (workspace + pages + macros).

Out of scope (handled by a separate skill — not this one):

- **Loupedeck plugin development** (C# or Node.js via the Logi Actions SDK at https://logitech.github.io/actions-sdk-docs/). Plugins ship the *catalog of action IDs* that this skill's profiles reference. If the user wants to develop a plugin for SPAD.next, a DAW, a MIDI controller (e.g. VAELTON GP180), that's plugin dev, not profile editing.

Typical user prompts:

- "Show me what's on touch page 2 of my MSFS profile"
- "Add a Discord push-to-talk button on the second touch page"
- "Make encoder 3 control brightness"
- "Copy my Sound Control page to all my other profiles"
- "Take the page named 'Windows shortcuts' and replace the same-named page in profil CT and profil Live"
- "Add a macro that launches GitHub Desktop"
- "Validate this profile before I import it"
- "Repack this folder back to .lp5"

## How to use it

All operations are Python scripts under `scripts/`. They take a **profile directory** (an unzipped `.lp5` or a copy of a runtime-store profile folder). Always **work on a copy**, never edit the live store directly unless the user explicitly asks.

### Standard workflow

1. **Extract** an `.lp5` to a working dir, **or** copy a runtime profile dir.
2. **Inspect** to confirm what's there.
3. **Edit** with one of the targeted scripts.
4. **Validate** (catches orphan refs, bad GUIDs, missing plugin declarations).
5. **Repack** to `.lp5` and ask the user to import via the Logi config UI.

### Scripts

| Script | Purpose |
|---|---|
| `scripts/lp5.py extract <file.lp5> <dest_dir>` | Unzip an `.lp5` to a working folder. |
| `scripts/lp5.py pack <src_dir> <file.lp5>` | Zip a folder back into `.lp5` (invalidates `ProfilePreview.json`). |
| `scripts/inspect.py <profile_dir>` | Pretty-print workspaces → pages → controls → action IDs. |
| `scripts/inspect.py <profile_dir> --json` | Same data as JSON for programmatic use. |
| `scripts/edit.py set-button <profile_dir> --workspace N --page N --index N --action ID` | Set a touch-page button action (also `--fn-action`). |
| `scripts/edit.py set-encoder <profile_dir> --workspace N --page N --index N [--press ID] [--rotate ID] [--fn-press ID] [--fn-rotate ID]` | Set encoder actions. |
| `scripts/edit.py set-round <profile_dir> --index N --action ID` | Set a round-button action (LD30 only). |
| `scripts/edit.py set-square <profile_dir> --index N --action ID [--fn-action ID]` | Set a square-button action. |
| `scripts/edit.py add-page <profile_dir> --workspace N --kind touch|encoder|wheel --name TITLE` | Append a new page to a workspace. |
| `scripts/edit.py delete-page <profile_dir> --page-id GUID` | Remove a page everywhere it's referenced. |
| `scripts/edit.py add-workspace <profile_dir> --name TITLE` | Add a fresh workspace (empty pages). |
| `scripts/macros.py add-launch-app <profile_dir> --name TITLE --exe PATH [--args "..."] [--cwd PATH]` | Add a macroCommand that launches an app. |
| `scripts/macros.py add-keyboard-action <profile_dir> --name TITLE --keys "Ctrl+Shift+P"` | Add a profileAction (keyboard shortcut). |
| `scripts/macros.py add-macro-keysequence <profile_dir> --name TITLE --keys "Ctrl+A" "Delete" "H"` | Add a macroCommand that fires a key sequence. |
| `scripts/macros.py list <profile_dir>` | List existing macros / profile actions. |
| `scripts/macros.py remove <profile_dir> --name <GUID-or-action-id>` | Delete a macro / adjustment / profile-action. |
| `scripts/propagate.py --src <dir> --src-page N --kind touch --dst <dir1> --dst <dir2> [--strategy clone-page\|replace-page] [--rename TITLE]` | **Cross-profile sync.** Copy a page (and dependent macros/icons/colors/plugin declarations) into one or more destination profiles. |
| `scripts/build_page.py <profile_dir> --sheet buttons.csv --images ./icons --workspace 0 --page-name "Quick launch" [--replace-named TITLE]` | **Build a page from a spreadsheet + image dir.** Each row = one button (kind ∈ `launch-app`, `keyboard`, `macro-keysequence`, `existing-action`, `empty`); icons become embedded `.ict` files. See `examples/`. |
| `scripts/validate.py <profile_dir>` | Cross-reference check + plugin declaration check + GUID format check. |

All scripts accept `--dry-run` to print the diff without writing. All write scripts save a `.bak` of `ProfileInfo.json` before mutating.

### Identifying the user's intent

When the user asks to change "button 3 of page 2", they almost always mean the **touch screen buttons** of the **current/home workspace**. Disambiguate only when ambiguous (e.g., CT has touch buttons *and* round/square buttons). The display layouts are:

- **Loupedeck CT / Loupedeck20**: 15 touch buttons per touch-page (3×5 grid), 6 encoders per encoder-page (3 left + 3 right), 8 round buttons, 12 square buttons, wheel.
- **Loupedeck Live / Loupedeck30**: 12 touch buttons per touch-page (3×4 grid), 6 encoders, 8 round, 12 square. *(Verify against the actual ProfileInfo.json's control count — that's authoritative.)*
- **Loupedeck Live S / Loupedeck40**: smaller; rely on the file's control count.

The **authoritative** control count is `len(touchPage.controls)`. Trust the file, not the spec.

## Important constraints (will silently break things if violated)

1. **GUIDs are 32 uppercase hex chars, no dashes**: `A969C71189314AE0BF5DAFC27D48CAF7`. Generate with `uuid.uuid4().hex.upper()`. Every new page/workspace/macro needs a fresh one — collisions silently corrupt references.
2. **Pages are cross-referenced by name**: a page exists once in `touchPages[]`/`encoderPages[]`/`wheelPages[]` and its `name` is listed in `workspaces[*].{touch,encoder,wheel}PageNames`. Deletion must clean both, plus `homeWorkspaceName` if it pointed there.
3. **`ActionColors.json` keys are composite**: `<actionId>|<workspaceGUID>|<pageGUID>`. The same nav-action has different colors per page. Don't flatten.
4. **Colors are encoded as decimal ints**: in `ActionColors.json` they look 24-bit RGB (`16711680 = 0xFF0000`); in `.ict` files they're 32-bit ARGB (`4278190080 = 0xFF000000`). Helper: `int(hexrgb, 16)` / `0xFF000000 | int(hexrgb, 16)`.
5. **Plugin declarations must match**: any non-`@Generic` plugin in an action ID must appear in `ProfileInfo.additionalNativePluginNames` AND `metadata/AdvancedInfo.json.additionalPluginNames`. `validate.py` checks this.
6. **`ProfilePreview.json` is a rendered cache** with PNG blobs. After edits, regenerate or delete it — the configui will rebuild it on import. Don't try to keep it in sync manually.
7. **Action ID grammar** is `$<plugin>___<className>[___<arg>]`. Special compound forms exist for navigation (`@ChangeTouchPage___main|<wsGUID>|<pageGUID>`) and dynamic folders (`#DynamicFolder___DynamicFolder#<className>`). See `reference/action-ids.md`.

## External references

- Loupedeck device features: https://support.loupedeck.com/loupedeck-device-features.html
- Plugin user guides: https://support.loupedeck.com/loupedeck-plugin-guides.html
- Plugin development (legacy, C#): https://support.loupedeck.com/loupedeck-plugin-development.html
- Plugin SDK tools for Windows: https://support.loupedeck.com/loupedeck-plugin-sdk-tools-for-windows.html
- **Logi Actions SDK (current)**: https://logitech.github.io/actions-sdk-docs/getting-started/ — supports both Node.js and C#.
  - C# plugin dev (richer API surface): https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/introduction/
  - Plugins extend the *catalog of action IDs* a profile can reference; this skill manages the profiles, not the plugins themselves. If a plugin's actions appear in a profile, the plugin's name must be declared (`additionalNativePluginNames` + `metadata/AdvancedInfo.json`).

There is no official documentation of the `.lp5` schema; this skill's `reference/schema.md` is the working reference, derived from real exported profiles.

## Safety defaults

- Default target is `.lp5` exports or a **copy** of a runtime profile dir.
- Writing directly to `%LOCALAPPDATA%\Logi\LogiPluginService\Applications\...` requires the user to pass `--allow-runtime` to any script. The Logi service may overwrite changes on the next restart or refuse them outright.
- Every write creates `<file>.bak`. Every script supports `--dry-run`.

## First-run sanity check (recommended)

The schema understanding is derived from real exports, not from official docs. Before relying on edited profiles, do a round-trip sanity check the first time:

1. Export an existing profile from the configui to `original.lp5`.
2. `python scripts/lp5.py extract original.lp5 ./work`
3. `python scripts/lp5.py pack ./work roundtrip.lp5`
4. Import `roundtrip.lp5` back via the configui.
5. Confirm the device behaves identically.

If step 5 fails, the issue is in pack/unpack round-trip (likely `ProfilePreview.json` handling), not in any edits — fix that before troubleshooting individual edits.

## Known limitations / experimental

- **`macros.py add-macro-keysequence`** is experimental: the pattern of a `macroCommands.actions[]` referring to per-step profileActions was not directly observed in the export corpus. Use `add-keyboard-action` for single shortcuts; for verified multi-step macros, build them in the configui once and inspect the result.
- **Cross-profile color sync**: `propagate.py` copies `ActionColors` entries whose base action ID matches, but it does not regenerate composite keys for the *new* page GUID when cloning. The configui assigns defaults; recolor in the UI if needed. *(TODO: rewrite composite keys with the new page GUID.)*
- **`keyboardKey` modifier `Ctrl=1`** is inferred — no Ctrl bindings in the corpus. Verified masks: `Alt=2, Shift=4, Win=8` (all match real exports byte-for-byte). If Ctrl shortcuts misfire after import, capture one in the configui and adjust `MODIFIER_DISPLAY` in `scripts/macros.py`.
- **No device/service is available in this skill's environment.** The skill cannot verify behavior on the physical device — every change must be confirmed by importing into the configui and exercising the device.
