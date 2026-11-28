#!/usr/bin/env python3
"""Build and run every complete program in the docs.

A complete program is a fenced block that opens with ```csharp Program.cs.
Each one becomes its own console or web project that references the local
CSharpEssentials projects (and their source generators). Console programs
must exit with code 0. A program that needs a server or a web host is only
built: web apps are detected from WebApplication, anything else opts out with
a "// build-only: <reason>" line.

Extra NuGet packages come from the block's own install lines, for example
"// dotnet add package Microsoft.EntityFrameworkCore.Sqlite". CSharpEssentials.*
install lines are skipped because every local project is referenced anyway.

Usage: python3 scripts/check-doc-examples.py [--build-only] [--keep] [paths...]
"""
import fcntl
import re
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
TFM = "net10.0"
FENCE = re.compile(r"^```csharp Program\.cs[^\n]*\n(.*?)^```", re.M | re.S)
ADD = re.compile(r"^//\s*dotnet add package\s+([\w.]+)(?:\s+--version\s+(\S+))?", re.M)
DOC_GLOBS = [".well-known/agent-skills/*/SKILL.md", "README.MD", "CSharpEssentials*/**/Readme.MD",
             "CSharpEssentials*/**/README.MD", "docs/**/*.md"]


def rel(p):
    try:
        return p.relative_to(ROOT)
    except ValueError:
        return p


def doc_files(args):
    if args:
        return [Path(a).resolve() for a in args]
    seen = []
    for g in DOC_GLOBS:
        for p in sorted(ROOT.glob(g)):
            if "/bin/" not in str(p) and "/obj/" not in str(p) and p not in seen:
                seen.append(p)
    return seen


def library_refs():
    refs = []
    for csproj in sorted(ROOT.glob("CSharpEssentials*/*.csproj")):
        name = csproj.stem
        if name.endswith(("Tests", "CodeFixes")) or name == "CSharpEssentials":
            continue
        if name.endswith("Generators"):
            refs.append(f'<ProjectReference Include="{csproj}" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />')
        else:
            refs.append(f'<ProjectReference Include="{csproj}" />')
    return refs


def package_versions():
    text = (ROOT / "Directory.Packages.props").read_text()
    best = {}
    for name, ver in re.findall(r'PackageVersion Include="([^"]+)" Version="([^"]+)"', text):
        key = tuple(int(x) if x.isdigit() else 0 for x in re.split(r"[.-]", ver.strip("[]()").split(",")[0]))
        if name not in best or key > best[name][0]:
            best[name] = (key, ver)
    return {k: v[1] for k, v in best.items()}


def main():
    argv = sys.argv[1:]
    build_only = "--build-only" in argv
    keep = "--keep" in argv
    paths = [a for a in argv if not a.startswith("--")]
    versions = package_versions()
    refs = "\n    ".join(library_refs())
    work = Path(tempfile.mkdtemp(prefix="cse-doc-examples-"))
    # Empty props files stop MSBuild from picking up anything above the work dir.
    (work / "Directory.Build.props").write_text("<Project />\n")
    (work / "Directory.Packages.props").write_text("<Project />\n")

    programs = []
    for doc in doc_files(paths):
        for i, m in enumerate(FENCE.finditer(doc.read_text())):
            code = m.group(1)
            line = doc.read_text()[: m.start()].count("\n") + 1
            name = f"p{len(programs):03d}"
            web = "WebApplication" in code
            pkgs = []
            for pkg, ver in ADD.findall(code):
                if pkg.startswith("CSharpEssentials"):
                    continue
                ver = ver or versions.get(pkg)
                if not ver:
                    print(f"{rel(doc)}:{line}: no version known for {pkg}; add --version")
                    return 1
                pkgs.append(f'<PackageReference Include="{pkg}" Version="{ver}" />')
            proj = work / name
            proj.mkdir()
            (proj / "Program.cs").write_text(code)
            (proj / f"{name}.csproj").write_text(f"""<Project Sdk="Microsoft.NET.Sdk{'.Web' if web else ''}">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{TFM}</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    {refs}
    {''.join(pkgs)}
  </ItemGroup>
</Project>
""")
            runnable = not web and "// build-only:" not in code
            programs.append((doc, line, proj, name, runnable))

    if not programs:
        print("No ```csharp Program.cs blocks found.")
        return 0
    slnx = "<Solution>\n" + "".join(f'  <Project Path="{p[3]}/{p[3]}.csproj" />\n' for p in programs) + "</Solution>\n"
    (work / "examples.slnx").write_text(slnx)
    print(f"Building {len(programs)} programs in {work}")
    # Parallel callers share the library obj folders, so builds take turns.
    (ROOT / "artifacts").mkdir(exist_ok=True)
    lock = open(ROOT / "artifacts" / ".doc-examples.lock", "w")
    fcntl.flock(lock, fcntl.LOCK_EX)
    build = subprocess.run(["dotnet", "build", str(work / "examples.slnx"), "-v:q", "-nologo", "-clp:ErrorsOnly"],
                           capture_output=True, text=True)
    failed = 0
    if build.returncode != 0:
        out = build.stdout + build.stderr
        for doc, line, proj, name, _ in programs:
            errs = [l for l in out.splitlines() if f"/{name}/" in l and "error" in l]
            if errs:
                failed += 1
                print(f"BUILD FAIL {rel(doc)}:{line}")
                for e in sorted(set(errs))[:8]:
                    print("   ", e.split(f"/{name}/", 1)[-1])
        if not failed:
            print(out[-4000:])
            return 1
    if not build_only:
        for doc, line, proj, name, runnable in programs:
            if not runnable:
                continue
            dll = proj / "bin" / "Debug" / TFM / f"{name}.dll"
            if not dll.exists():
                continue
            try:
                r = subprocess.run(["dotnet", str(dll)], capture_output=True, text=True, timeout=60, cwd=proj)
                if r.returncode != 0:
                    failed += 1
                    print(f"RUN FAIL {rel(doc)}:{line} exit {r.returncode}")
                    print("   ", (r.stderr or r.stdout).strip()[-800:])
            except subprocess.TimeoutExpired:
                failed += 1
                print(f"RUN TIMEOUT {rel(doc)}:{line}")
    if not keep:
        subprocess.run(["rm", "-rf", str(work)])
    print(f"{len(programs) - failed}/{len(programs)} programs OK")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
