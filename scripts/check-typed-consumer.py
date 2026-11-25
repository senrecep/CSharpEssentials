#!/usr/bin/env python3
"""Build every case of tests/TypedResultConsumer with the real SDK and check the C# diagnostics it reports.

The in-process Roslyn harness in tests/CSharpEssentials.Results.Typed.Tests is not the compiler a consumer builds with. This
script builds against the packed packages in artifacts/feed with the SDK pinned in global.json and compares the set of CS and
CSE diagnostic ids of each case with the expected set (the CSE ids come from the analyzers packed into the Results package, so a
case that expects one proves they load): a case that expects none must build clean with TreatWarningsAsErrors on,
a case that expects ids must fail with exactly those. Every case is built for each target framework of the fixture: net11.0 uses
the runtime UnionAttribute and IUnion, net10.0 the internal polyfill shipped in the package.

Usage, from the repository root after packing to artifacts/feed:
    python3 scripts/check-typed-consumer.py --version <packed version>
"""

import argparse
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / "tests" / "TypedResultConsumer"

CASES = {
    "AllArms": set(),
    "MissingArm": {"CS8509"},
    "DuplicateSlot": {"CS0457"},
    "ForeignError": {"CS0029"},
    "ReturningStatement": {"CS0161"},
    "TypedAllArms": set(),
    "TypedMissingArm": {"CS8509"},
    "TypedDuplicateSlot": {"CS0457", "CSE5002"},
    "TypedForeignError": {"CS0029"},
    "TypedReturningStatement": {"CS0161"},
    "TypedAsyncChain": set(),
    "TypedQuerySyntax": set(),
    "TypedDefault": {"CSE5001"},
}

FRAMEWORKS = ("net11.0", "net10.0")

DIAGNOSTIC = re.compile(r"\b(?:error|warning) (CSE?\d{4})\b")


def build(case: str, framework: str, version: str, configuration: str) -> tuple[int, set[str], str]:
    result = subprocess.run(
        [
            "dotnet", "build", str(PROJECT),
            "--framework", framework,
            "--configuration", configuration,
            "--no-incremental",
            f"-p:FixtureCase={case}",
            f"-p:CseVersion={version}",
        ],
        cwd=ROOT,
        capture_output=True,
        text=True,
        check=False,
    )
    output = result.stdout + result.stderr
    return result.returncode, set(DIAGNOSTIC.findall(output)), output


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--version", required=True, help="the version passed to dotnet pack")
    parser.add_argument("--configuration", default="Release")
    args = parser.parse_args()

    on_disk = {path.stem for path in (PROJECT / "Cases").glob("*.cs")}
    if on_disk != set(CASES):
        print(f"::error::tests/TypedResultConsumer/Cases has {sorted(on_disk)}, the script expects {sorted(CASES)}")
        return 1

    failed = 0
    for framework in FRAMEWORKS:
        for case, expected in CASES.items():
            code, actual, output = build(case, framework, args.version, args.configuration)
            built = code == 0
            if actual == expected and built == (not expected):
                print(f"ok   {framework} {case}: {sorted(actual) or 'clean build'}")
                continue

            failed += 1
            print(f"::error::{framework} {case}: expected {sorted(expected) or 'a clean build'}, got {sorted(actual)} (exit code {code})")
            print(output)

    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
