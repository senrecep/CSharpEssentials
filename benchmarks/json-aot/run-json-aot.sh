#!/usr/bin/env bash
# Native AOT check for the generated polymorphism path: publishes JsonAot (events in its own assembly, IEvent in JsonAotCore,
# a JsonSerializerContext plus AddPolymorphism, no reflection API) with Native AOT, fails on any IL, C# or CSE warning, then runs the
# binary, which exits 0 only when the round trips match.
# Usage: run-json-aot.sh
set -euo pipefail

HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
WORK=$(mktemp -d)

cleanup() {
    rm -rf "$HERE/JsonAot/bin" "$HERE/JsonAot/obj" "$HERE/JsonAotCore/bin" "$HERE/JsonAotCore/obj" "$WORK"
}
trap cleanup EXIT

# Clean first: an incremental publish skips CoreCompile and drops the analyzer warnings.
# PublishAot comes from the csproj: as a global -p it would also reach the netstandard2.0 generator project (NETSDK1207).
cleanup
mkdir -p "$WORK"
log="$WORK/publish.log"
dotnet publish "$HERE/JsonAot/JsonAot.csproj" -c Release -r linux-x64 -p:TreatWarningsAsErrors=false \
    -p:TrimmerSingleWarn=false -o "$WORK/out" --tl:off -clp:NoSummary >"$log" 2>&1 || { cat "$log"; exit 1; }

warnings=$(grep -E "warning (IL|CS|CSE|SYSLIB)[0-9]+" "$log" | sort -u || true)
if [[ -n $warnings ]]; then
    echo "$warnings"
    echo "Warnings: $(wc -l <<<"$warnings")"
    exit 1
fi

echo "publish: 0 IL, C# or CSE warnings, binary $(awk -v b="$(stat -c %s "$WORK/out/JsonAot")" 'BEGIN {printf "%.1f MB", b / 1048576}')"
"$WORK/out/JsonAot"
