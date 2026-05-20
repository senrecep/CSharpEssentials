#!/usr/bin/env bash
# Run all benchmarks for every supported .NET framework and store results separately.
#
# Usage:
#   ./run-benchmarks.sh                      # full run — all TFMs, all scenarios, max precision (~60 min)
#   ./run-benchmarks.sh --quick              # fast run — net9 only, key scenarios, Short job (~3-5 min)
#   ./run-benchmarks.sh --quick --tfm net10.0  # fast run on a specific TFM
#   ./run-benchmarks.sh -- --filter "*Wide*" # pass extra BenchmarkDotNet args
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/CSharpEssentials.Validation.Benchmarks/CSharpEssentials.Validation.Benchmarks.csproj"
RESULTS_DIR="$SCRIPT_DIR/results"

# Defaults
QUICK=false
CUSTOM_TFM=""
EXTRA_ARGS=()

# Parse arguments
while [[ $# -gt 0 ]]; do
    case "$1" in
        --quick)   QUICK=true; shift ;;
        --tfm)     CUSTOM_TFM="$2"; shift 2 ;;
        --)        shift; EXTRA_ARGS+=("${@}"); break ;;
        *)         EXTRA_ARGS+=("$1"); shift ;;
    esac
done

if [[ "$QUICK" == "true" ]]; then
    # Quick mode: single TFM, representative scenarios, Short job (~3-5 min total)
    DEFAULT_TFM="net9.0"
    FRAMEWORKS=("${CUSTOM_TFM:-$DEFAULT_TFM}")
    FILTER="*Simple*|*Wide*|*Complex*|*Cascade*|*Construction*|*LargeCollection*"
    JOB_ARGS=(--job Short)
    MODE_LABEL="QUICK (${FRAMEWORKS[0]}, key scenarios, Short job)"
else
    # Full mode: all TFMs, all scenarios, full precision
    FRAMEWORKS=("net9.0" "net10.0" "net11.0")
    if [[ -n "$CUSTOM_TFM" ]]; then
        FRAMEWORKS=("$CUSTOM_TFM")
    fi
    FILTER="*"
    JOB_ARGS=()
    MODE_LABEL="FULL (${FRAMEWORKS[*]})"
fi

mkdir -p "$RESULTS_DIR"

echo ""
echo "======================================================="
echo "  Mode: $MODE_LABEL"
echo "======================================================="

for TFM in "${FRAMEWORKS[@]}"; do
    echo ""
    echo "  Running benchmarks on $TFM..."
    ARTIFACTS="$RESULTS_DIR/$TFM"
    mkdir -p "$ARTIFACTS"

    if dotnet run -c Release \
        --framework "$TFM" \
        --project "$PROJECT" \
        -- \
        --filter "$FILTER" \
        --exporters json github \
        --artifacts "$ARTIFACTS" \
        "${JOB_ARGS[@]}" \
        "${EXTRA_ARGS[@]}" \
        2>&1 | tee "$ARTIFACTS/run.log"; then
        echo "  ✓ $TFM completed — results in $ARTIFACTS/results/"
    else
        echo "  ✗ $TFM failed — see $ARTIFACTS/run.log"
    fi
done

echo ""
echo "======================================================="
echo "  All runs complete. Results per framework:"
for TFM in "${FRAMEWORKS[@]}"; do
    echo "    $TFM → $RESULTS_DIR/$TFM/results/"
done
echo "======================================================="
