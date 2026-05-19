# Action ID grammar

Every button/encoder/wheel references an *action* by string. Format:

```
$<plugin>___<className_or_id>[___<arg>]
```

- `$` is always the first char.
- `___` (triple underscore) is the segment separator.
- `<plugin>` is the plugin name (`@Generic`, `DefaultWin`, `Msfs`, `MidiBuilder`, ...).
- `@`-prefixed plugin = built-in / generic.

## Built-in `@Generic` actions seen in real profiles

| Action ID prefix | Purpose |
|---|---|
| `$@Generic___@Macro___<GUID>` | Reference a `macroCommands[]` entry by its GUID. |
| `$@Generic___@MacroAdjustment___<GUID>` | Reference a `macroAdjustments[]` entry (encoder). |
| `$@Generic___@ProfileAction___<GUID>` | Reference a `profileActions[]` entry. |
| `$@Generic___@KeyboardKey` | Template name for keyboard shortcut actions (used as `templateActionName`, not directly bound). |
| `$@Generic___@ExecuteApplication___<exe>\|\|<args>\|\|<cwd>` | Launch an application. `\|\|` separates exe/args/cwd. Empty segments still keep the separators. |
| `$@Generic___@ChangeTouchPage___<modeName>\|<workspaceGUID>\|<pageGUID>` | Switch to a specific touch page. |
| `$@Generic___@ChangeEncoderPage___<modeName>\|<workspaceGUID>\|<pageGUID>` | Switch to a specific encoder page. |
| `$@Generic___@ChangeWorkspace___<modeName>\|<workspaceGUID>` | Switch to a specific workspace. |
| `$@Generic___@PreviousTouchPage` / `@NextTouchPage` | Cycle pages. |
| `$@Generic___@PreviousEncoderPage` / `@NextEncoderPage` | Cycle encoder pages. |
| `$@Generic___@ButtonClock` | Built-in analog clock display. |
| `$@Generic___@MouseWheel` | Wheel adjustment for scrolling. |
| `$@Generic___Loupedeck.GenericPlugin.StopwatchDynamicAction` | Stopwatch. |
| `$@Generic___Loupedeck.GenericPlugin.DateTimeDynamicAction___Date` | Date display. |
| `$@Generic___Loupedeck.GenericPlugin.DateTimeDynamicAction___WeekNumber` | Week number. |

## `DefaultWin` plugin (Windows-system actions)

| Action ID | Purpose |
|---|---|
| `$DefaultWin___Volume` | Volume adjust (encoder). |
| `$DefaultWin___ResetVolume` | Mute toggle (press). |
| `$DefaultWin___Brightness` / `ResetBrightness` | Display brightness. |
| `$DefaultWin___MediaPlayPause` | Media play/pause. |
| `$DefaultWin___LockWorkstation` | Win+L. |
| `$DefaultWin___AddDesktop`, `CloseDesktop` | Virtual desktops. |
| `$DefaultWin___WindowsExplorer`, `WindowsSearch`, `WindowsEmoji`, `WindowsScreenshot`, `WindowsActions` | Shell shortcuts. |
| `$DefaultWin___#DynamicFolder___DynamicFolder#Loupedeck.DefaultWinPlugin.VolumeMixerOutputDynamicFolder` | Per-app volume mixer. |
| `$DefaultWin___#DynamicFolder___DynamicFolder#Loupedeck.DefaultWinPlugin.TaskSwitcherDynamicFolder` | Task switcher folder. |

## Dynamic folders

Some plugins expose folders of dynamically generated buttons (e.g. one button per running app). Format:

```
$<plugin>___#DynamicFolder___DynamicFolder#<fully.qualified.FolderClassName>
```

Example: `$Msfs___#DynamicFolder___DynamicFolder#Loupedeck.MsfsPlugin.folder.APDynamicFolder`.

## Plugin-supplied actions (Msfs example)

Plugins expose actions whose IDs use their `.NET` class names:

- `$Msfs___Loupedeck.MsfsPlugin.SpeedInput`
- `$Msfs___Loupedeck.MsfsPlugin.SpeedAPEncoder` (rotate target)
- `$Msfs___ResetLoupedeck.MsfsPlugin.SpeedAPEncoder` (press = reset)
- `$Msfs___Loupedeck.MsfsPlugin.folder.APMultiInputs___AP Alt` — folder + sub-action via `___<arg>`.

## `keyboardKey` encoding

Used inside `actionParameters.parameters.keyboardKey`. Format:

```
<canonical>___<modifierMask>___<display>___<platform>-<vk>#¤%&+?<vk-or-flag>#¤%&+?<modifierMask>#¤%&+?<vk2?>
```

Real examples:

```
AltOrOption+Tab___67896332___Alt+Tab___win-9#¤%&+?2#¤%&+?67896332#¤%&+?15
Windows+ArrowUp___67896332___Win+ArrowUp___win-38#¤%&+?8#¤%&+?67896332#¤%&+?72
Windows+Shift+ArrowRight___67896332___Win+Shift+ArrowRight___win-39#¤%&+?12#¤%&+?67896332#¤%&+?77
```

`scripts/macros.py` builds these from friendly strings. Heuristics observed:
- The `67896332` token recurs as a stable separator/marker. Keep it literal.
- `win-<vk>` carries the Windows virtual-key code.
- Modifier names: `Ctrl`, `Shift`, `AltOrOption` (NOT `Alt`), `Windows`. Display side renders them as `Alt`, `Win`.
- Modifier mask values seen: `2` (Alt), `4` (Ctrl), `6` (Alt+Shift), `8` (Win), `12` (Win+Shift).

## Composing references safely

When you add a macro/profileAction/page and want to **point a button at it**, you write into a `controls[i].pressAction` field one of:

- `$@Generic___@Macro___<macro GUID>` — for `macroCommands` entries.
- `$@Generic___@MacroAdjustment___<adj GUID>` — for `macroAdjustments` entries.
- `$@Generic___@ProfileAction___<pa GUID>` — for `profileActions` entries. **Note**: the `name` field of a `profileActions` entry is the *full action ID itself*, e.g. `$@Generic___@ProfileAction___AA32B3A5527B4DF9A60A4D901EEBE219`, not just the GUID. That's the inconsistency that bites you: macros store the bare GUID in `name`, profileActions store the full action ID in `name`.

`scripts/macros.py` hides this asymmetry behind a uniform API.
