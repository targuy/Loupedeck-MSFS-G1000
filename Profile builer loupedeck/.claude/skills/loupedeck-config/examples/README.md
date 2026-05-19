# Examples — build a page from a sheet

The sheet describes one button per row. Run `build_page.py` to materialise the page.

## Column reference

| column | required | meaning |
|---|---|---|
| `index` | yes | button slot on the page (0-based; max is page size − 1, usually 14 on CT, 11 on Live). |
| `display_name` | yes for non-`empty` rows | label that lands on the macro/profile-action and on the `.ict` text overlay (currently overlay text empty by default — set in the script if you want it visible). |
| `icon` | optional | filename inside `--images <dir>`. Square PNG ~90×90 or 60×60 works well; the configui resizes. |
| `kind` | yes | one of: `launch-app`, `keyboard`, `macro-keysequence`, `existing-action`, `empty`. |
| `action_id` | when `kind=existing-action` | full action ID, e.g. `$DefaultWin___LockWorkstation` or `$@Generic___@ProfileAction___<GUID>`. |
| `exe` | when `kind=launch-app` | executable path (Windows path or `shell:AppsFolder\…`). |
| `args` | optional | for `launch-app`. |
| `cwd` | optional | for `launch-app`. |
| `keys` | when `kind=keyboard` / `macro-keysequence` | single combo (`Windows+Shift+S`) or pipe-separated combos for sequences. |
| `color` | optional | `#RRGGBB` — written to `ActionColors.json`. |
| `fn_action_id` | optional | a second action bound to the Fn modifier on the same button. |

## Run

```bash
cd "E:/DocumentsBenoit/pythonProject/loupedeck"

# Dry-run first to see what would be built
python .claude/skills/loupedeck-config/scripts/build_page.py \
    /tmp/lptest/ct_copy \
    --sheet .claude/skills/loupedeck-config/examples/example_page.csv \
    --images ./my_icons \
    --workspace 0 \
    --page-name "Quick launch" \
    --dry-run

# For real
python .claude/skills/loupedeck-config/scripts/build_page.py \
    /tmp/lptest/ct_copy \
    --sheet .claude/skills/loupedeck-config/examples/example_page.csv \
    --images ./my_icons \
    --workspace 0 \
    --page-name "Quick launch"
```

To replace (re-use the same GUID, useful for keeping references working):

```bash
python .claude/skills/loupedeck-config/scripts/build_page.py <profile_dir> \
    --sheet new_buttons.csv --images ./my_icons \
    --workspace 0 --page-name "Quick launch" --replace-named "Quick launch"
```

## Tips

- **Excel users**: save as CSV UTF-8 or paste into a `.tsv` (Tab-Separated Values). `.xlsx` works too if `openpyxl` is installed (`pip install openpyxl`).
- **Empty cells** are fine — only fill what the row's `kind` needs.
- **`kind=empty`** explicitly reserves a slot but leaves it unbound (useful if you want gaps in the grid).
- The script **validates the whole sheet before writing anything** — invalid rows abort the build cleanly.
- Use `--dry-run` first whenever the sheet changes.
