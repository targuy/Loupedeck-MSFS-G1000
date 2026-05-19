# Architecture Notes

Phase 1 starts from the validated spike results, not from the original plan verbatim.

## Confirmed

- Logi Plugin Service SDK is local `PluginApi.dll` version `6.3.0.2406`.
- WASimCommander `1.3.2.0` validates `C# -> WASimCommander -> MSFS -> callback -> C#`.
- The real WASim update periods are `Never`, `Once`, `Tick`, `Millisecond`; there is no `Changed`.
- CT wheel integration works through assignable `PluginDynamicAdjustment` controls in wheel page templates.
- Bitmap colors render correctly on device.

## Corrected From Plan

- Do not use a hypothetical `SetWheelScreen()` API.
- Do not assume direct RGB physical LED setters exist.
- Treat physical button LED behavior as action/layout-dependent until a dedicated physical-button spike proves otherwise.
- Use `PluginDynamicAdjustment` for CT wheel page state, with optional `PluginConfiguration2` defaults later.

## Phase 1 Shape

- `SimLayer` is the facade used by plugin actions and state managers.
- `ISimClient` keeps the WASimCommander implementation replaceable and testable.
- `WaSimReflectionClient` is the first production sim client. It loads `WASimCommander.WASimClient.dll` at runtime, connects to simulator + server, executes calculator code, and forwards subscription callbacks.
- `NullSimClient` remains the fallback when the WASimCommander client DLL is missing, so the plugin can still load and log commands.
- `G1000StateManager` mirrors subscribed sim state.
- `PluginRuntime` owns the active `SimLayer` and `G1000StateManager`, and relays state changes to actions.
- `AutopilotMasterCommand` now uses a command + telemetry loop: it sends `AP_MASTER`, then redraws from the subscribed `AUTOPILOT MASTER` state callback.
- `G1000MappedCommand` and `G1000MappedAdjustment` expose the G1000 page controls from a central table instead of one class per button.
- `MsfsMappedCommand` and `MsfsMappedAdjustment` expose generic MSFS K:Events through WASim so users can build their own pages in the Loupedeck UI.
- Concrete MSFS/G1000 actions are now preferred for manual profiles because they are directly visible in Logi UI. They can render subscribed sim values such as AP modes, selected altitude, heading bug, gear, flaps, lights, and radio frequencies.
- Device adapters record Live/CT behavior differences without depending on unvalidated LED APIs.
- Initial production actions can be introduced one by one while the final Dynamic Folder is still under design.

## Packaging

- If `.external/WASimCommander/SDK/lib/managed/net8` exists locally, the build copies `WASimCommander.WASimClient.dll`, `Ijwhost.dll`, and `client_conf.ini` into the plugin output.
- The source repository still does not commit those WASimCommander distribution files.
