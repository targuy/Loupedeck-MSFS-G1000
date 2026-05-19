"""Extract and pack .lp5 archives.

Usage:
    python lp5.py extract <file.lp5> <dest_dir>
    python lp5.py pack <src_dir> <file.lp5> [--keep-preview]

An `.lp5` is a plain ZIP. Packing drops `metadata/ProfilePreview.json` by default
(the configui regenerates it on import; stale previews can confuse the UI).
"""
from __future__ import annotations

import argparse
import sys
import zipfile
from pathlib import Path


def extract(src: Path, dest: Path) -> None:
    if not src.is_file():
        sys.exit(f"error: not a file: {src}")
    dest.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(src) as zf:
        zf.extractall(dest)
    print(f"extracted {src} -> {dest}")
    _summarize_dir(dest)


def pack(src: Path, dest: Path, *, keep_preview: bool) -> None:
    if not src.is_dir():
        sys.exit(f"error: not a directory: {src}")
    if not (src / "ProfileInfo.json").is_file():
        sys.exit(f"error: {src} has no ProfileInfo.json — not a profile dir")
    dest.parent.mkdir(parents=True, exist_ok=True)
    files = sorted(p for p in src.rglob("*") if p.is_file())
    if not keep_preview:
        files = [p for p in files if p.name != "ProfilePreview.json"]
    with zipfile.ZipFile(dest, "w", compression=zipfile.ZIP_DEFLATED) as zf:
        for f in files:
            zf.write(f, f.relative_to(src).as_posix())
    print(f"packed {src} -> {dest} ({len(files)} files)")


def _summarize_dir(d: Path) -> None:
    parts = []
    if (d / "ApplicationInfo.json").exists():
        parts.append("ApplicationInfo.json")
    if (d / "ProfileInfo.json").exists():
        parts.append("ProfileInfo.json")
    if (d / "ActionColors").exists():
        parts.append(f"ActionColors/ ({sum(1 for _ in (d / 'ActionColors').iterdir())})")
    if (d / "ActionIcons").exists():
        parts.append(f"ActionIcons/ ({sum(1 for _ in (d / 'ActionIcons').iterdir())})")
    if (d / "ActionImages").exists():
        parts.append(f"ActionImages/ ({sum(1 for _ in (d / 'ActionImages').iterdir())})")
    if (d / "metadata").exists():
        parts.append("metadata/")
    print("  contents:", ", ".join(parts))


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)

    e = sub.add_parser("extract", help="unzip a .lp5")
    e.add_argument("src")
    e.add_argument("dest")

    p = sub.add_parser("pack", help="zip a profile dir to .lp5")
    p.add_argument("src")
    p.add_argument("dest")
    p.add_argument("--keep-preview", action="store_true",
                   help="keep metadata/ProfilePreview.json (default: drop it)")

    args = ap.parse_args()
    if args.cmd == "extract":
        extract(Path(args.src), Path(args.dest))
    elif args.cmd == "pack":
        pack(Path(args.src), Path(args.dest), keep_preview=args.keep_preview)


if __name__ == "__main__":
    main()
