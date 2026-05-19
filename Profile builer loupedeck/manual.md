# Loupedeck profile editor — user manual

A beginner-friendly guide. Read this if you've never used the skill before.

---

## 1. What this is, in plain English

You own a Loupedeck (Logi) device — a CT, Live, Live S, etc. The "configui" app that came with it lets you customise every button, knob, page, and macro, but doing it click-by-click is painful, especially when you want the *same* "Sound Control" or "Windows shortcuts" page in *every* one of your profiles.

This skill is a set of helpers that Claude can run for you. You **talk to Claude** like a person; Claude runs the helpers under the hood. You don't have to touch a terminal unless you want to. The skill:

- reads your `.lp5` exports
- changes them — adds pages, buttons, macros; copies pages between profiles; builds pages from a spreadsheet
- writes a new `.lp5` you can import back into the Loupedeck app

Nothing happens to your live device or your live profiles until **you** import the resulting `.lp5` through the Loupedeck app. The skill never writes to the live install by default.

---

## 2. The 4 words you need to know

| word | meaning |
|---|---|
| **Profile** | One mapping of "what every control does", for one specific app (e.g. one profile for MSFS, one for default Windows). |
| **Workspace** | A sub-section of a profile. You can have several workspaces in a profile and switch between them on the device. |
| **Page** | A screen of buttons (or knobs). A workspace contains several pages. |
| **Action** | What a button or knob does when you press/turn it. E.g. "launch Excel" or "Win+Shift+S". |

The Loupedeck has three kinds of controls:

- **Touch buttons** (BV in our sheets) — the icons on the LCD touch strip.
- **Knobs** (K) — the 6 rotary dials. Each can press AND rotate.
- **Physical buttons** (BP) — the round and square buttons around the edges.

---

## 3. The five things you can ask Claude to do

Just tell Claude what you want. No special syntax.

1. **"Show me what's in this profile"** — Claude reads it, summarises it.
2. **"Change button X on page Y to do Z"** — Claude edits it.
3. **"Copy this page from my Live profile to my CT and MSFS profiles"** — cross-profile sync.
4. **"Build a page from this spreadsheet"** — fill `template_filled.xlsx`, hand it to Claude.
5. **"Make me a macro that launches Outlook"** — Claude creates the macro and binds it.

The skill always works on **copies** of your `.lp5` files. Your originals are never touched.

---

## 4. A real conversation — adding a Quick-launch page

Imagine this is your screen. **You type the bold lines; Claude does the rest.**

---

> **You**: I exported my CT default profile to `C:\Users\benoi\Documents\profil default CT.lp5`. Can you add a new page called "Quick launch" with these buttons: Excel, Outlook, Screenshot (Win+Shift+S), Lock Windows.

> **Claude**: Sure. Let me look at the profile first.
>
> *(Claude runs `lp5.py extract` and `show.py` behind the scenes)*
>
> The profile is for Windows default apps, has one workspace called "Workspace (1)" with 3 touch pages (5 spaces left). I'll add a new "Quick launch" page at the end of that workspace, then bind 4 buttons:
>
> - Slot 0: Excel (launch app)
> - Slot 1: Outlook (launch app)
> - Slot 2: Screenshot (Windows + Shift + S)
> - Slot 3: Lock Workstation (built-in action `$DefaultWin___LockWorkstation`)
>
> The other 11 slots stay empty. Want any icon images, or should the buttons just show the labels?

> **You**: I have icons in `C:\Users\benoi\Documents\loupedeck-icons`. Filenames: `excel.png`, `outlook.png`, `snip.png`, `lock.png`.

> **Claude**: Good. Building now.
>
> *(Claude builds the page via `build_page.py`, then `validate.py`, then `lp5.py pack`)*
>
> Done. New file: `C:\Users\benoi\Documents\profil default CT_quicklaunch.lp5`
>
> Validation: clean (no errors). 4 buttons bound + 4 `.ict` icons embedded.
>
> To use it:
> 1. Open the Loupedeck configui.
> 2. Right-click your CT default profile → **Duplicate** (so the original is safe).
> 3. Profile menu → **Import profile** → pick `profil default CT_quicklaunch.lp5`.
> 4. Switch to it on the device; the "Quick launch" page is in Workspace (1), last page.

> **You**: Worked. Excel and Outlook launch. But the screenshot button does nothing.

> **Claude**: That's a known thing — I encode the keyboard combo using bit masks I verified against the corpus. The `Ctrl` modifier value isn't in the corpus and is a guess. Can you do this once in the configui: bind any button to **Ctrl+A**, export the profile, and tell me the path? I'll read the real combo and update the encoder.

> *…you do that, send the path…*

> **Claude**: Got it. Confirmed `Ctrl=2` (not 1 as I'd guessed). Fixed in `scripts/macros.py`. Rebuilding the screenshot button — and the rest of the page that used keyboard combos.

> *(another build cycle…)*

> **You**: Perfect, all 4 work.

---

That's it. **You never opened a terminal.** You opened the configui twice — once to export, once to import.

---

## 5. A second conversation — copying a page to all your profiles (Goal #2)

> **You**: I have a "Sound control" touch page in my Live profile. I want exactly the same page in my CT profile and my MSFS profile, so I don't have to switch profiles on the device just to mute volume.

> **Claude**: Easy. Where are the three `.lp5` files?

> **You**: `C:\Users\benoi\Documents\profil default Live.lp5`, `…\profil default CT.lp5`, `…\msfs24spadnext.lp5`.

> **Claude**: One second.
>
> *(extracts all three; runs `show.py` on Live to find the page; runs `propagate.py` with `--src Live --dst CT --dst MSFS --strategy replace-page` or `clone-page`)*
>
> In your Live profile, "Sound control" is touch page 1 of workspace "Daily Use". It references the built-in `DefaultWin` volume actions, no custom macros — so the copy is straightforward.
>
> Do you want:
> - **(a)** the page appended as a new page in each target profile (safer; keeps anything currently there), or
> - **(b)** if a same-named page already exists, replace it in place (so future re-syncs overwrite it)?

> **You**: (b). I might re-run this often.

> **Claude**: Done. Output files:
>
> - `profil default CT_synced.lp5`
> - `msfs24spadnext_synced.lp5`
>
> Both validate clean. Carried over: 0 macros (none needed), 6 icons. Import each via the configui as before.

---

## 6. A third conversation — building from your spreadsheet

You have `template_filled.xlsx` open with green example rows pre-filled. Pretend you've overwritten K1, K2, and BV1–BV6 with your own choices, leaving the rest as `empty`.

> **You**: Here's my filled spreadsheet: `E:\DocumentsBenoit\pythonProject\loupedeck\template_filled.xlsx`. Apply it to my CT default profile.

> **Claude**: Reading the metadata block first…
>
> - device_type: Loupedeck20 ✓ matches your profile.
> - workspace_name: (blank) → I'll use the first workspace ("Workspace (1)").
> - touch_page_name: "My touch page" — should I rename that?

> **You**: Call it "Cockpit".

> **Claude**: OK. encoder_page_name stays "My encoder page" or rename too?

> **You**: "Cockpit knobs".

> **Claude**: replace_existing=true → if "Cockpit"/"Cockpit knobs" pages already exist they'll be overwritten. Sound right?

> **You**: Yes.

> **Claude**: Building now.
>
> *(reads sheet → groups rows by control kind → builds touch page from BV rows, encoder page from K/LK rows, wheel from WHEEL row, ignores empty rows → bakes icons → propagate plugin declarations → validates → packs)*
>
> Result: `profil default CT_from_sheet.lp5`. Validation clean. Bound 2 knobs + 6 touch buttons + WHEEL. Import as usual.

---

## 7. Testing it the very first time (do this once)

Before you trust the skill with edits, do a **zero-change round-trip** so you confirm the configui accepts what we pack.

> **You**: First-time sanity test. Take my `profil default CT.lp5`, extract it, pack it back with no changes, and give me the result.

> **Claude**: One second.
>
> *(extract → pack → emit `_roundtrip.lp5`)*
>
> Output: `profil default CT_roundtrip.lp5`. Import it in the configui (after duplicating the original for safety). Switch to it on the device. Every button, knob, page should behave exactly like the original.

If that test passes, the pack/unpack pipeline is sound. From that point you can trust any edits I make.

If it fails (the configui rejects the import or a button stops working), **stop and tell me what happened** — the failure is in the round-trip itself, not your edits, and we need to fix that first.

---

## 8. Backups & safety

- The skill **never** edits your original `.lp5` files. It always works on extracted copies in a temp folder.
- Every script that changes a profile makes a `.bak` copy of `ProfileInfo.json` in the same folder, before writing.
- Before importing a generated `.lp5` into the configui, **duplicate the target profile in the configui first**. The configui's "Duplicate" is one click; if anything goes wrong you switch back to the duplicate.
- If you have to manually recover: `ProfileInfo.json.bak` next to the modified one is the previous version. Rename `.json.bak` → `.json` to roll back.

---

## 9. Cheat sheet — what to say to Claude

| You want to… | Try saying… |
|---|---|
| See what's in a profile | "Show me what's in `<path>.lp5`" |
| Round-trip test | "Extract and repack `<path>.lp5` with no changes" |
| Change one button | "On my MSFS profile, set touch page 2 button 5 to play/pause music" |
| Add a launch-app button | "Add an Outlook button to page 1 of my CT default profile" |
| Add a keyboard shortcut | "Make a button for Ctrl+Shift+P on page 3 of my Live profile" |
| Copy a page to other profiles | "Take page 1 of my Live profile and put it in my CT and MSFS profiles too" |
| Build from a spreadsheet | "Apply my `template_filled.xlsx` to my CT profile, page name 'Cockpit'" |
| Check a profile is valid | "Validate `<profile path>`" |
| Look up an action ID | "What's the action ID for the Windows lock screen?" |

You can also just describe what you want in free-form and Claude will figure out which scripts to call.

---

## 10. Where things live

```
E:\DocumentsBenoit\pythonProject\loupedeck\
├── manual.md                        ← this file
├── template_filled.xlsx             ← the spreadsheet builder
├── profiles exported\               ← your real exports (sample data)
└── .claude\
    └── skills\
        └── loupedeck-config\
            ├── SKILL.md             ← Claude reads this for the "how"
            ├── reference\           ← deep schema docs
            ├── scripts\             ← the actual Python tools
            └── examples\            ← example CSV
```

You never have to touch the `.claude\skills\…` folder. It's there so Claude can use it.

---

## 11. Known caveats

- **`Ctrl` modifier mask** in keyboard shortcuts is a guess (`Ctrl=1`). The other modifiers (Alt=2, Shift=4, Win=8) are verified. If a Ctrl+something shortcut doesn't fire after import, tell Claude — we have a calibration procedure (export a known Ctrl binding from the configui, diff against ours).
- **Multi-step macros** (the `macro-keysequence` kind) use a pattern that wasn't seen in your three sample profiles. They *should* work but treat them as experimental until you confirm one fires correctly.
- **Page previews**: the configui regenerates them on import; you might see a brief moment with no icon before it catches up.
- **Cross-profile color sync**: when a page is *cloned* (new GUID), per-page color settings get default values in the destination. If you want exact colors carried over, re-colour in the configui after import.

---

## 12. When something goes wrong

Tell Claude:

1. **The exact command/conversation** that led to the problem.
2. **What you expected vs what actually happened.**
3. For configui errors: a screenshot of the dialog.
4. For device behaviour issues: which button/knob/profile, what you pressed, what fired (or didn't).

Don't try to hand-edit `ProfileInfo.json` first — give Claude the raw failure to see.
