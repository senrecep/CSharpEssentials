#!/usr/bin/env python3
"""Generate the Docs7 site in docs7/ from the repository's Markdown sources.

Usage: python3 scripts/docs7.py           regenerate docs7/
       python3 scripts/docs7.py --check   exit 1 when docs7/ is out of date
"""
import json
import re
import shutil
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "docs7"
BLOB = "https://github.com/senrecep/CSharpEssentials/blob/main/"
RAW = "https://raw.githubusercontent.com/senrecep/CSharpEssentials/main/"
SITE = {
    "$schema": "https://mintlify.com/docs.json",
    "theme": "mint",
    "name": "CSharpEssentials",
    "colors": {"primary": "#58a6ff", "light": "#0969da", "dark": "#58a6ff"},
    "navbar": {"primary": {"type": "github", "href": "https://github.com/senrecep/CSharpEssentials"}},
    "footer": {"socials": {"github": "https://github.com/senrecep/CSharpEssentials"}},
}
DOCS_GROUPS = [
    ("Migration", "migration"),
    ("Design", "design"),
    ("Architecture decisions", "adr"),
    ("Blog notes", "blog-notes"),
]
LINK = re.compile(r"(!?\[[^\]]*\]\()([^)\s]+)(\))")
IMG_TAG = re.compile(r'(<img\s[^>]*src=")([^"]+)(")')


def slug(name):
    name = name.removeprefix("CSharpEssentials").lstrip(".") or "CSharpEssentials"
    return name.lower().replace(".", "-")


def collect():
    pages = {ROOT / "README.MD": "index", ROOT / "docs/API_REFERENCE.md": "API_REFERENCE"}
    groups = {"Packages": [], "Examples": [], "Benchmarks": []}
    for pkg in sorted(ROOT.glob("CSharpEssentials*/")):
        for readme in sorted(p for p in pkg.rglob("*") if p.name.lower() == "readme.md"
                             and "bin" not in p.parts and "obj" not in p.parts):
            sub = readme.parent.relative_to(pkg).parts
            page = "packages/" + "-".join([slug(pkg.name), *[s.lower() for s in sub]])
            pages[readme] = page
            groups["Packages"].append(page)
        bench = pkg / "BENCHMARKS.md"
        if bench.exists():
            page = "benchmarks/" + slug(pkg.name)
            pages[bench] = page
            groups["Benchmarks"].append(page)
    for readme in sorted(ROOT.glob("examples/*/README.md")):
        page = "examples/" + readme.parent.name.removeprefix("Examples.").lower().replace(".", "-")
        pages[readme] = page
        groups["Examples"].append(page)
    nav = [{"group": "Get started", "pages": ["index", "API_REFERENCE"]}]
    nav += [{"group": g, "pages": p} for g, p in groups.items()]
    for group, folder in DOCS_GROUPS:
        found = sorted((ROOT / "docs" / folder).glob("*.md"))
        for md in found:
            pages[md] = f"{folder}/{md.stem}"
        nav.append({"group": group, "pages": [pages[md] for md in found]})
    return pages, nav


def rewrite(target, source, pages):
    if re.match(r"^[a-z]+:|^#|^/", target):
        return target
    path, _, anchor = target.partition("#")
    resolved = (source.parent / path).resolve()
    hit = next((page for src, page in pages.items() if src.resolve() == resolved), None)
    if hit:
        return "/" + hit + ("#" + anchor if anchor else "")
    try:
        rel = resolved.relative_to(ROOT).as_posix()
    except ValueError:
        return target
    base = RAW if resolved.suffix.lower() in {".png", ".svg", ".jpg", ".gif"} else BLOB
    return base + rel + ("#" + anchor if anchor else "")


def convert(source, pages):
    text = source.read_text(encoding="utf-8-sig")
    text = re.sub(r"\A\s*<p align=\"center\">.*?</p>\s*", "", text, flags=re.S)
    match = re.search(r"^# (.+)$", text, flags=re.M)
    title = match.group(1).strip() if match else source.stem
    if match:
        text = text[:match.start()] + text[match.end():]
    out, fenced = [], False
    for line in text.lstrip("\n").splitlines():
        if line.lstrip().startswith("```"):
            fenced = not fenced
        elif not fenced:
            line = LINK.sub(lambda m: m.group(1) + rewrite(m.group(2), source, pages) + m.group(3), line)
            line = IMG_TAG.sub(lambda m: m.group(1) + rewrite(m.group(2), source, pages) + m.group(3), line)
        out.append(line)
    front = ["---", "title: " + json.dumps(title, ensure_ascii=False)]
    if pages[source] == "index":
        intro = next(l for l in out if l and not l.startswith(("[", "!", "<")))
        front.append("description: " + json.dumps(intro.strip(), ensure_ascii=False))
    return "\n".join(front + ["---", ""] + out).rstrip() + "\n"


def build(dest):
    pages, nav = collect()
    for source, page in pages.items():
        target = dest / (page + ".mdx")
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(convert(source, pages), encoding="utf-8")
    site = dict(SITE, navigation={"groups": nav})
    (dest / "docs.json").write_text(json.dumps(site, indent=2) + "\n", encoding="utf-8")


def snapshot(folder):
    return {p.relative_to(folder).as_posix(): p.read_bytes() for p in folder.rglob("*") if p.is_file()}


def main():
    if "--check" in sys.argv:
        with tempfile.TemporaryDirectory() as tmp:
            build(Path(tmp))
            expected, actual = snapshot(Path(tmp)), snapshot(OUT) if OUT.exists() else {}
        stale = sorted(k for k in expected.keys() | actual.keys() if expected.get(k) != actual.get(k))
        if stale:
            print("docs7/ is out of date; run python3 scripts/docs7.py. Changed: " + ", ".join(stale))
            return 1
        print("docs7/ is up to date.")
        return 0
    shutil.rmtree(OUT, ignore_errors=True)
    build(OUT)
    print(f"Wrote {sum(1 for _ in OUT.rglob('*.mdx'))} pages to docs7/.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
