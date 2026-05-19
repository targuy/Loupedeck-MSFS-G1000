# Phase 0 Spikes

This project is intentionally limited to feasibility validation before implementing the final G1000 plugin.

## S1+S3 - WASimCommander bidirectional communication

Location: `spikes/LoupedeckMSFSG1000.Spikes.WaSimBidi`

Purpose:

- Load `WASimCommander.WASimClient.dll` at runtime without redistributing it.
- Connect to MSFS through the user-installed WASimCommander module.
- Subscribe to `(A:AUTOPILOT MASTER, bool)`.
- Send `(>K:AP_MASTER)`.
- Measure and document observed callback behavior and latency.

Run shape:

```powershell
dotnet run --project spikes/LoupedeckMSFSG1000.Spikes.WaSimBidi -- "C:\Path\To\WASimCommander.WASimClient.dll"
```

If the official SDK was downloaded to `.external/WASimCommander`, the spike auto-detects:

```text
.external/WASimCommander/SDK/lib/managed/net8/WASimCommander.WASimClient.dll
```

It also copies `Ijwhost.dll` and `client_conf.ini` to the spike runtime folder when available.

MSFS 2024 Game Pass module path on this machine:

```text
C:\Users\benoi\AppData\Local\Packages\Microsoft.Limitless_8wekyb3d8bbwe\LocalCache\Packages\Community\wasimcommander-module
```

Installed local binary distribution:

```text
.external/WASimCommander/WASimCommander_SDK-v1.3.2.0.zip
.external/WASimCommander/WASimModule-v1.3.2.0.zip
```

`.external/` is ignored by Git because the WASimCommander binaries should not be redistributed in this repository.

Observed dry-run without MSFS loaded:

```text
connectSimulator result: FAIL
Simulator connection failed. Start MSFS 2024, load a flight, and confirm the WASimCommander module is green in DevMode > WASM.
```

Observed live run with MSFS 2024 loaded:

```text
connectSimulator result: OK
connectServer result: OK
saveDataRequest result: OK
Subscribed to: (A:AUTOPILOT MASTER, bool)
Sending: (>K:AP_MASTER)
Data callback: ... Data: 00-00-00-00-00-00-00-00
Data callback: ... Data: 00-00-00-00-00-00-F0-3F
```

Interpretation:

- `00-00-00-00-00-00-00-00` is double `0.0`.
- `00-00-00-00-00-00-F0-3F` is double `1.0`.
- The end-to-end path `C# -> WASimCommander -> MSFS -> callback -> C#` is validated for `AP_MASTER`.
- The plan assumption `UpdatePeriod.Changed` is incorrect for WASimCommander 1.3.2.0; the available enum is `Never`, `Once`, `Tick`, `Millisecond`.

Next live validation steps:

1. Start MSFS 2024.
2. Load an aircraft into a flight.
3. Enable Developer Mode and check `Options > WASM`; `wasimcommander-module` should be green.
4. Run:

```powershell
dotnet run --project spikes/LoupedeckMSFSG1000.Spikes.WaSimBidi
```

Decision gate:

- L:Var/softkey availability is blocking for the final G1000 softkey implementation.
- WASim latency, LED color, and wheel screen API remain validation items but are not blocking architecture decisions.

## S2 - Dynamic Folder and BitmapBuilder

Location: `src/LoupedeckMSFSG1000/Spikes/DynamicFolderSpike.cs`

Purpose:

- Validate that a `PluginDynamicFolder` can be loaded.
- Validate `BitmapBuilder` rendering on Loupedeck button surfaces.
- Capture button interactions and redraw command images.

Current limitation:

- Physical RGB LED control is not implemented because the public SDK surface found locally does not expose an obvious per-button LED setter. This remains a hardware/API probe.
- Device observation confirmed no physical button LEDs light up for this spike, but the current spike does not assign actions to physical buttons. The better interpretation is: physical LED behavior is still unvalidated under valid physical-button action conditions.

Installed package validation:

```text
logiplugintool install .\bin\LoupedeckMSFSG1000.lplug4
Plugin was successfully installed
```

Logi Plugin Service load log:

```text
Dynamic folder loaded: 'LoupedeckMSFSG1000.Spikes.DynamicFolderSpike'
Dynamic action added: 'LoupedeckMSFSG1000.Spikes.Phase0StatusCommand'
Plugin 'LoupedeckMSFSG1000' version '0.1.0' loaded
```

Manual device validation:

1. Open the Loupedeck/Logi configuration app.
2. Find plugin `Loupedeck MSFS G1000`.
3. Find group `Phase 0`.
4. Drag `G1000 Phase 0 Spike` to a touch button.
5. Enter the dynamic folder on the Loupedeck.
6. Confirm the folder icon renders a blue `G1000` bitmap.
7. Confirm the inner commands render green/amber bitmap buttons.
8. Press `Status`; its counter should increment and redraw.
9. Press `LED/API`; its counter should increment and a warning should be written in `Logs/plugin_logs/LoupedeckMSFSG1000.log`.

S2 is considered validated when the folder is visible on hardware and button redraws occur without plugin-service errors.

Observed device result:

- Dynamic folder appears on hardware.
- Bitmap colors render correctly.
- Button counters redraw correctly.
- Text rendering is acceptable only with explicit area-based drawing; early coordinate-only rendering was visually poor.
- Physical button LEDs remain off in this spike. Hypothesis: LEDs may require actual physical-button action assignments, which this spike does not yet create.

## S2b - CT wheel screen

Status: validated on Loupedeck CT.

The local SDK exposes CT wheel concepts (`WheelTool` and wheel pages), but not the `SetWheelScreen()` API assumed in the plan. The implementation must use official wheel abstractions instead of hard-coding `SetWheelScreen()`.

Implemented spike:

```text
src/LoupedeckMSFSG1000/Spikes/Phase0WheelTool.cs
```

SDK findings:

- `WheelTool.CreateImage()` is the bitmap rendering hook.
- `WheelTool.OnEncoderEvent(...)` is the wheel interaction hook.
- Both are `protected` overrides.
- A `PluginDynamicFolder` can expose wheel tools with `GetWheelToolNames(DeviceType)`.
- The spike returns `LoupedeckMSFSG1000.Phase0Wheel` from `DynamicFolderSpike.GetWheelToolNames(...)`.

Build/package status:

```text
dotnet build LoupedeckMSFSG1000.slnx
dotnet test LoupedeckMSFSG1000.slnx --no-restore
logiplugintool pack
logiplugintool verify
logiplugintool install
```

All commands pass.

Manual CT validation:

1. Use a Loupedeck CT.
2. Place `G1000 Phase 0 Spike` on a touch button.
3. Enter the dynamic folder.
4. Check whether `G1000 Wheel Spike` appears as a wheel tool/page.
5. Confirm the wheel screen shows page colors: `PFD`, `MFD`, `AP`, `COM`.
6. Rotate the CT wheel and confirm the page label/color cycles.

If a prebuilt/default CT wheel page is needed later, use `PluginConfiguration2.PluginLayouts[*].WheelPages`. For now the validated path is user drag-and-drop into CT wheel page templates.

Observed CT result:

- Placing `G1000 Page Cycle` in a CT wheel page works.
- Rotating the wheel changes the icon/page state (`PFD`, `MFD`, `AP`, `COM`).
- Placing the same adjustment on another encoder also works; the icon appears next to that encoder rather than on the central wheel page.
- Placing the dynamic folder itself in a wheel page executes/opens the folder but does not replace the wheel page rendering.

Conclusion:

- The correct CT integration path is `PluginDynamicAdjustment` / assignable actions inside CT wheel page templates.
- The original plan's `SetWheelScreen()` assumption is invalid for the local SDK and should be removed from the architecture.
- `WheelTool` exists and compiles, but the practical user-facing route is the drag-and-drop adjustment/action model exposed by the CT UI.
