# Loupedeck profile schema (`ProfileInfo.json`)

Derived from real exports (CT/LD20 and Live/LD30). Logitech does not publish an official spec.

## Top-level fields

```jsonc
{
  "$type": "Loupedeck.Service.ApplicationProfile, LoupedeckService",
  "name": "<32-hex-upper, no dashes>",       // profile GUID
  "profileFlags": "None",
  "displayName": "MSFS2024",
  "description": null,
  "deviceType": "Loupedeck20" | "Loupedeck30" | ...,
  "applicationName": "spad" | "default" | "...",
  "nativePluginName": null,
  "hasNativePlugin": false,
  "additionalNativePluginNames": ["DefaultWin", "Msfs"],
  "lastModifiedTimeUtc": "2026-02-20T17:22:07.11198Z",
  "profileSettings": {
    "$type": "Loupedeck.DictionaryNoCase`1[[System.String, ...]], PluginApi",
    "midi": "true",
    "hiddenPluginNames": ""
  },
  "actionImages90": null,
  "actionImages60": null,
  "wheelImages": null,
  "actionColors": null,
  "layout": { /* see below */ },
  "macroCommands": [ /* see below */ ],
  "macroAdjustments": [ /* see below */ ],
  "profileCommands": [],
  "profileAdjustments": [],
  "conversionHistory": "2026-02-20T15:58:42Z as 6.2.6.1611\r\n",
  "packageName": "<profile GUID>",
  "packageVersion": "1.0.9548.32564",
  "profileActions": [ /* see below */ ]
}
```

## `layout`

```jsonc
{
  "$type": "Loupedeck.Service.ProfileLayout20, LoupedeckService",
  "deviceType": "Loupedeck20",
  "profileFlags": "None",
  "layoutModes": [
    {
      "$type": "Loupedeck.Service.ProfileLayoutMode20, LoupedeckService",
      "deviceType": "Loupedeck20" | "None",
      "modeName": "main" | "System" | "<custom>",
      "parentModeName": null,
      "actions": null,
      "dynamicButtonPages": null,
      "dynamicEncoderPages": null,

      "touchPages":   [ ProfileLayoutButtonPage,  ... ],
      "encoderPages": [ ProfileLayoutEncoderPage, ... ],
      "wheelPages":   [ ProfileLayoutWheelPage,   ... ],

      "workspaces": [
        {
          "$type": "Loupedeck.Service.ProfileLayoutWorkspace20, LoupedeckService",
          "name": "<GUID>",
          "displayName": "Workspace (1)",
          "description": null,
          "touchPageNames":   ["<pageGUID>", ...],
          "encoderPageNames": ["<pageGUID>", ...],
          "wheelPageNames":   ["<pageGUID>", ...],
          "activationActions": []
        }
      ],
      "homeWorkspaceName": "<workspace GUID>"
    }
  ],
  "roundPage":  ProfileLayoutButtonPage,   // physical round-buttons row (CT/Live)
  "squarePage": ProfileLayoutButtonPage    // physical square-buttons row (CT/Live)
}
```

## Page shapes

### `ProfileLayoutButtonPage` (touch / round / square)

```jsonc
{
  "$type": "Loupedeck.Service.ProfileLayoutButtonPage, LoupedeckService",
  "name": "<page GUID>",
  "displayName": "Page tactile (2)",
  "description": null,
  "controls": [
    {
      "$type": "Loupedeck.Service.ProfileLayoutButton, LoupedeckService",
      "pressAction":   "$<plugin>___<action>" | null | "",
      "fnPressAction": "$<plugin>___<action>" | null
    }
    // ... fixed length per device+page-kind
  ],
  "dynamicPageName": null,
  "dynamicPagePluginName": null,
  "dynamicPageNumber": 0
}
```

Control count by page kind (see `devices.md` for the table):
- LD20 touchPage: 15. LD20 roundPage: 8. LD20 squarePage: 12.
- LD30 touchPage: 12 (verify in file). LD30 roundPage: 8. LD30 squarePage: 12.

### `ProfileLayoutEncoderPage`

```jsonc
{
  "$type": "Loupedeck.Service.ProfileLayoutEncoderPage, LoupedeckService",
  "name": "<page GUID>",
  "displayName": "Dial Page (1)",
  "description": null,
  "controls": [
    {
      "$type": "Loupedeck.Service.ProfileLayoutEncoder, LoupedeckService",
      "pressAction":   "$<plugin>___<action>" | null,
      "fnPressAction": null,
      "rotateAction":  "$<plugin>___<action>" | null,
      "fnRotateAction": null
    }
    // 6 entries on CT/Live
  ],
  "dynamicPageName": null,
  "dynamicPagePluginName": null,
  "dynamicPageNumber": 0
}
```

### `ProfileLayoutWheelPage` (CT wheel)

```jsonc
{
  "$type": "Loupedeck.Service.ProfileLayoutWheelPage, LoupedeckService",
  "name": "<page GUID>",
  "displayName": "Clock",
  "description": null,
  "templateName": "WheelToolAnalogClock",
  "parameters": {
    "$type": "Loupedeck.StringDictionaryNoCase, PluginApi",
    "actions": "$@Generic___@ButtonClock",
    "adjustment": "$@Generic___@MouseWheel"
  }
}
```

## Macros and profile actions

### `macroCommands` — multi-step push commands

```jsonc
{
  "$type": "Loupedeck.Service.ApplicationProfileMacroCommand, LoupedeckService",
  "isCommand": true,
  "name": "<32-hex-upper GUID>",
  "displayName": "Apple Music",
  "description": "",
  "groupName": "",
  "superGroupName": "@macro",
  "supportedOs": "All",
  "supportedModes": ["system"],
  "showAsSingleAction": true,
  "actionEditorCommands": [],
  "isMultiState": false,
  "actions": [
    "$@Generic___@ExecuteApplication___<exe>||<args>||<cwd>"
    // OR action-references that resolve to other plugin actions
  ]
}
```

The composed action ID for using this macro from a button: `$@Generic___@Macro___<name GUID>`.

### `macroAdjustments` — encoder-style macros (left turn / right turn / press-reset)

```jsonc
{
  "$type": "Loupedeck.Service.ApplicationProfileMacroAdjustment, LoupedeckService",
  "isCommand": false,
  "name": "<GUID>",
  "displayName": "taskswitch",
  "actionEditorCommands": [
    {
      "$type": "Loupedeck.Service.MacroActionEditorCommand, LoupedeckService",
      "name": "<short slug>",                   // internal ref, not a GUID
      "templateName": "$@Generic___@KeyboardKey",
      "actionParameters": {
        "$type": "System.Collections.Generic.Dictionary`2[[System.String,...],[System.String,...]], System.Private.CoreLib",
        "keyboardKey": "AltOrOption+Shift+Tab___67896332___Alt+Shift+Tab___win-9#¤%&+?6#¤%&+?67896332#¤%&+?15"
      }
    }
  ],
  "actionsBefore": [""],
  "actionsLeft":   ["<editor command slug>"],
  "actionsRight":  ["<editor command slug>"],
  "actionsReset":  [],
  "clickRateLimit": 9,
  /* ... */
}
```

The composed action ID: `$@Generic___@MacroAdjustment___<name GUID>`.

### `profileActions` — single keyboard/system actions reusable across the profile

```jsonc
{
  "$type": "Loupedeck.Service.ApplicationProfileCommand, LoupedeckService",
  "isCommand": true,
  "name": "$@Generic___@ProfileAction___<GUID>",   // note: full action ID is the name here
  "templateActionName": "$@Generic___@KeyboardKey",
  "actionParameters": {
    "$type": "Loupedeck.ActionEditorActionParameters, PluginApi",
    "parameters": {
      "$type": "Loupedeck.StringDictionaryNoCase, PluginApi",
      "keyboardKey": "Windows+ArrowUp___67896332___Win+ArrowUp___win-38#¤%&+?8#¤%&+?67896332#¤%&+?72"
    },
    "count": 1
  },
  "displayName": "maximiser",
  "description": "Activate a keyboard shortcut with a single press or hold ...",
  "groupName": "",
  "superGroupName": "@macro",
  "isProfileAction": true,
  "isMultiState": false,
  "isResetCommand": false,
  "adjustmentName": null,
  "states": null
}
```

The `keyboardKey` field has a barbaric custom encoding:
`<canonical>___<modifier mask?>___<display>___<platform>-<vk>#¤%&+?<modifiers>#¤%&+?<mask?>#¤%&+?<vk>`.
Helpers in `scripts/macros.py` build it from a friendly string like `"Ctrl+Shift+P"`.

## Sibling files

### `ActionColors/ActionColors.json`

Composite keys, RGB int values (24-bit):

```jsonc
{
  "$@Generic___@ChangeTouchPage___main|<workspaceGUID>|<pageGUID>": 16711680,   // 0xFF0000
  "$@Generic___@ChangeEncoderPage___main|<workspaceGUID>|<pageGUID>": 16711680
}
```

The `|` separator is part of the key. Same nav-action under different pages gets different colors.

### `ActionIcons/<actionId>.ict`

JSON with a stack of image/text "items" over a background. Values that look like 32-bit ARGB ints:

```jsonc
{
  "backgroundColor": 4278190080,   // 0xFF000000 (opaque black)
  "items": [
    {
      "image": "<base64 PNG>",
      "imageFileName": "",
      "imageColor": 4294967295,    // 0xFFFFFFFF
      "imageRotation": "None",
      "isVisible": true,
      "itemType": "Image",
      "area": { "x": 0, "y": 0, "width": 100, "height": 100 }
    },
    {
      "text": "",
      "originalText": null,
      "textColor": 4294967295,
      "fontSize": 4,
      "fontName": "Arial",
      "isVisible": true,
      "itemType": "Text",
      "area": { "x": 0, "y": 81, "width": 100, "height": 19 }
    }
  ]
}
```

The `area` units appear to be percentages (0–100).

### `ActionImages/<actionId>.png`

Simple raster icon — bypasses the `.ict` compositor.

### `metadata/LoupedeckPackage.yaml`

```yaml
type: Profile5
name: <profile GUID>
displayName: MSFS2024
version: 1.0.9635.30903
```

### `metadata/AdvancedInfo.json`

```json
{ "additionalPluginNames": ["Msfs"] }
```

### `metadata/ProfilePreview.json`

Big precomputed render cache (per button: actionId, displayName, base64 PNG). The configui regenerates this when you import. Easiest correct behaviour: **delete it before/after editing** and let the configui rebuild on first open.

### `ApplicationInfo.json`

Identifies the target application:

```json
{
  "$type": "Loupedeck.Service.SupportedApplicationInfo, LoupedeckService",
  "name": "spad",
  "displayName": "SPAD.neXt",
  "deviceType": "Loupedeck20",
  "processOrBundleName": "spad.next",
  "modes": [{ "name": "main", "displayName": "Main" }],
  "defaultProfileName": "<profile GUID>",
  "isEnabled": true
}
```

## Live runtime vs export

The live runtime store at `%LOCALAPPDATA%\Logi\LogiPluginService\Applications\<DeviceType>\<appName>\Profiles\<profileGUID>\` mirrors the *contents* of an `.lp5`, just unzipped and without `ApplicationInfo.json` (that lives one level up at `Applications\<DeviceType>\<appName>\ApplicationInfo.json`).

Don't edit the runtime store while the Logi service is running. The safe round-trip is: export → edit → import via configui.
