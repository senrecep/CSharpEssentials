#!/usr/bin/env bash
# Native AOT probe for the packages this repo marks IsAotCompatible (Time, These, Rules, Entity, Clone, Resilience,
# Http, Idempotency, RequestResponseLogging). Publishes AotProbe from a clean obj with TrimmerSingleWarn=false (set in
# the csproj), lists the distinct IL warnings with their source, then runs the native binary, which prints
# "OK <package>" for each package and exits 0 only when every check passes.
# Usage: run-aot-probe.sh   Env: STRICT=1 keeps the repo's warnings-as-errors on, so any IL warning fails the publish.
set -euo pipefail

HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
PROJ="$HERE/AotProbe/AotProbe.csproj"
WORK=$(mktemp -d)

cleanup() { rm -rf "$HERE/AotProbe/bin" "$HERE/AotProbe/obj" "$WORK"; }
trap cleanup EXIT

# Clean first: an incremental publish skips CoreCompile and drops the analyzer warnings.
rm -rf "$HERE/AotProbe/bin" "$HERE/AotProbe/obj"
twae=()
[[ ${STRICT:-0} == 1 ]] || twae=(-p:TreatWarningsAsErrors=false)
dotnet publish "$PROJ" -c Release -r linux-x64 -m:2 "${twae[@]}" -o "$WORK/out" --tl:off -clp:NoSummary >"$WORK/publish.log" 2>&1 || {
    cat "$WORK/publish.log"
    exit 1
}

echo "IL warnings (distinct):"
grep -E 'warning IL[0-9]+' "$WORK/publish.log" | sed -E 's/ \[[^]]*\]$//; s#^ *##; s#'"${HERE%/benchmarks/*}"'/##' | cut -c1-200 | sort -u || echo "none"

bin="$WORK/out/AotProbe"
echo "Native binary: $(awk -v b="$(stat -c %s "$bin")" 'BEGIN {printf "%.1f MB", b / 1048576}')"
"$bin"
