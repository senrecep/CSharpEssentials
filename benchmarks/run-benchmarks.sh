#!/usr/bin/env bash
# Run all benchmarks for every supported .NET framework and store results separately.
#
# Usage:
#   ./run-benchmarks.sh                      # full run — all TFMs, all scenarios, max precision (~60 min)
#   ./run-benchmarks.sh --quick              # fast run — net9 only, key scenarios, Short job (~10 min)
#   ./run-benchmarks.sh --quick --tfm net10.0  # fast run on a specific TFM
#   ./run-benchmarks.sh -- --filter "*Wide*" # pass extra BenchmarkDotNet args
#   ./run-benchmarks.sh --project results    # Results async matrix benchmarks (default: validation)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Defaults
QUICK=false
CUSTOM_TFM=""
SUITE="validation"
EXTRA_ARGS=()

# Parse arguments
while [[ $# -gt 0 ]]; do
    case "$1" in
        --quick)   QUICK=true; shift ;;
        --tfm)     CUSTOM_TFM="$2"; shift 2 ;;
        --project) SUITE="$2"; shift 2 ;;
        --)        shift; EXTRA_ARGS+=("${@}"); break ;;
        *)         EXTRA_ARGS+=("$1"); shift ;;
    esac
done

# BenchmarkDotNet treats the whole --filter value as a single glob (no '|' union),
# so quick-mode key scenarios are run one pattern at a time.
case "$SUITE" in
    validation)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Validation.Benchmarks/CSharpEssentials.Validation.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results"
        ALL_FRAMEWORKS=("net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*Simple*" "*Wide*" "*Complex*" "*Cascade*" "*Construction*" "*LargeCollection*")
        ;;
    results)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Results.Benchmarks/CSharpEssentials.Results.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/results-pattern"
        ALL_FRAMEWORKS=("net8.0" "net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*MapBenchmarks*" "*BindBenchmarks*" "*TapBenchmarks*" "*MatchBenchmarks*" "*EnsureBenchmarks*" "*ThenBenchmarks*" "*TraverseBenchmarks*" "*RenamedMemberBenchmarks*")
        ;;
    *)
        echo "Unknown --project '$SUITE' (expected: validation | results)" >&2
        exit 1
        ;;
esac

if [[ "$QUICK" == "true" ]]; then
    # Quick mode: single TFM, representative scenarios, Short job (~3-5 min total)
    DEFAULT_TFM="net9.0"
    FRAMEWORKS=("${CUSTOM_TFM:-$DEFAULT_TFM}")
    PATTERNS=("${QUICK_PATTERNS[@]}")
    JOB_ARGS=(--job Short)
    MODE_LABEL="QUICK ($SUITE, ${FRAMEWORKS[0]}, key scenarios, Short job)"
else
    # Full mode: all TFMs, all scenarios, full precision
    FRAMEWORKS=("${ALL_FRAMEWORKS[@]}")
    if [[ -n "$CUSTOM_TFM" ]]; then
        FRAMEWORKS=("$CUSTOM_TFM")
    fi
    PATTERNS=("*")
    JOB_ARGS=()
    MODE_LABEL="FULL ($SUITE, ${FRAMEWORKS[*]})"
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

    FAILED=false
    for PATTERN in "${PATTERNS[@]}"; do
        echo "  → filter: $PATTERN"
        if dotnet run -c Release \
            --framework "$TFM" \
            --project "$PROJECT" \
            -- \
            --filter "$PATTERN" \
            --exporters json github \
            --artifacts "$ARTIFACTS" \
            "${JOB_ARGS[@]}" \
            "${EXTRA_ARGS[@]}" \
            2>&1 | tee -a "$ARTIFACTS/run.log"; then
            :
        else
            FAILED=true
        fi
    done

    if [[ "$FAILED" == "false" ]]; then
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
