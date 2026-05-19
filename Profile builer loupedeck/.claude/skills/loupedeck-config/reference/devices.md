# Device control counts

The **authoritative** count is always `len(page.controls)` in the profile file. This table is for orientation only.

| deviceType   | Model name       | touchPage | encoderPage | roundPage | squarePage | wheel |
|--------------|------------------|----------:|------------:|----------:|-----------:|:-----:|
| Loupedeck10  | (older Live)     | ?         | ?           | -         | -          | no    |
| Loupedeck15  | (older)          | ?         | ?           | -         | -          | no    |
| Loupedeck20  | **CT**           | 15        | 6           | 8         | 12         | yes   |
| Loupedeck30  | **Live**         | 12        | 6           | 8         | 12         | no    |
| Loupedeck40  | **Live S**       | ?         | ?           | ?         | ?          | no    |
| Loupedeck50  | (variant)        | ?         | ?           | ?         | ?          | ?     |
| Loupedeck60  | (variant)        | ?         | ?           | ?         | ?          | ?     |
| Loupedeck70  | (variant)        | ?         | ?           | ?         | ?          | ?     |
| Loupedeck71  | (variant)        | ?         | ?           | ?         | ?          | ?     |
| Loupedeck72  | (variant)        | ?         | ?           | ?         | ?          | ?     |

The numeric `deviceType` suffix appears to be a stable internal ID, not the visible model number.

The full device-folder list comes from `%LOCALAPPDATA%\Logi\LogiPluginService\Applications\` on a machine with the service installed.

## CT (Loupedeck20) layout overview

- **Touch screen**: 3 rows × 5 columns = 15 LCD buttons per touch-page.
- **Encoders**: 6 rotary encoders (3 left + 3 right of the touch screen) per encoder-page. Each has press / fn-press / rotate / fn-rotate.
- **Round buttons**: 8 physical buttons (page/workspace navigation by default). Single page (`layout.roundPage`).
- **Square buttons**: 12 physical buttons. Single page (`layout.squarePage`). Common defaults: media keys, page nav, fn modifier.
- **Wheel**: round haptic wheel with one or more `wheelPages` (e.g. `WheelToolAnalogClock` template).
- **Fn modifier**: every button/encoder has a paired `fn*` action triggered while the Fn key is held.

## Live (Loupedeck30) layout overview

- **Touch screen**: 3 rows × 4 columns = 12 LCD buttons per touch-page (verify in file).
- **Encoders**: 6 rotary encoders.
- **Round buttons**: 8 (page/workspace navigation).
- **Square buttons**: 12.
- **No wheel.**

## How to disambiguate "button 3 of page 2" with the user

In order of likelihood:

1. **Touch screen button** of the current/home workspace's page 2. (Almost always what's meant.)
2. **Encoder** of encoder-page 2 (if the user said "knob" or "dial").
3. **Round button** (rare — usually only edited to remap workspace nav).
4. **Square button** (rare — usually media-keys and fn-pairs).

If unsure, ask. The cost of one clarification is small; silently editing the wrong control is annoying to undo.
