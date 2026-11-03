#!/usr/bin/env bash
# Run all benchmarks for every supported .NET framework and store results separately.
#
# Usage:
#   ./run-benchmarks.sh                      # full run — all TFMs, all scenarios, max precision (~60 min)
#   ./run-benchmarks.sh --quick              # fast run — one TFM (net9.0; net10.0 for endpoints), key scenarios, Short job (~10 min)
#   ./run-benchmarks.sh --quick --tfm net10.0  # fast run on a specific TFM
#   ./run-benchmarks.sh -- --filter "*Wide*" # pass extra BenchmarkDotNet args
#   ./run-benchmarks.sh --project results    # Results async matrix benchmarks (default: validation)
#   ./run-benchmarks.sh --project idempotency  # Idempotency middleware and fingerprint benchmarks
#   ./run-benchmarks.sh --project endpoints    # Endpoint mapping startup and request throughput vs Carter / FastEndpoints
#   ./run-benchmarks.sh --project results-comparison  # Result/Maybe operations vs CSharpFunctionalExtensions / ErrorOr / FluentResults
#   ./run-benchmarks.sh --project di           # Service registration, decoration and resolve vs Scrutor
#   ./run-benchmarks.sh --project mediator     # Send, publish, startup and behavior cost: MediatR vs Mediator (+ CSE behaviors)
#   ./run-benchmarks.sh --project enums        # ToString, parse, IsDefined, GetValues, flags, description vs Enums.NET / NetEscapades.EnumGenerators / BCL
#                                              (net8.0 runs CSE's netstandard2.1 build: the package has no net8 target)
#   ./run-benchmarks.sh --project any          # Any construct/match/try-pick/partition/JSON/ToString vs OneOf / Dunet, These vs hand-written record
#                                              (net8.0 runs CSE's netstandard2.1 build: the packages have no net8 target)
#   ./run-benchmarks.sh --project json         # Json enum/polymorphic/DateTime vs System.Text.Json / Macross.Json.Extensions; Core case/Guider/random/WithCancellation vs BCL / Humanizer / ShortGuid
#                                              (net8.0 runs CSE's netstandard2.1 builds and System.Text.Json 9.0.x from NuGet)
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

# Quick-mode TFM unless a suite overrides it below.
DEFAULT_TFM="net9.0"

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
    idempotency)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Idempotency.Benchmarks/CSharpEssentials.Idempotency.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/idempotency"
        ALL_FRAMEWORKS=("net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*MiddlewareOverheadBenchmarks*" "*FingerprintBufferingBenchmarks*")
        ;;
    endpoints)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Endpoints.Benchmarks/CSharpEssentials.Endpoints.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/endpoints"
        ALL_FRAMEWORKS=("net8.0" "net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*Startup*" "*Throughput*")
        # Carter 10 and the AddValidation POST rows need net10.0, so quick mode defaults to it for this suite.
        DEFAULT_TFM="net10.0"
        ;;
    results-comparison)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Results.Comparison.Benchmarks/CSharpEssentials.Results.Comparison.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/results-comparison"
        ALL_FRAMEWORKS=("net8.0" "net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*CreateComparison*" "*MapComparison*" "*BindComparison*" "*EnsureComparison*" "*MatchComparison*" "*TapComparison*" "*AsyncPipelineComparison*" "*ErrorAggregationComparison*" "*MaybeComparison*")
        ;;
    di)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.DependencyInjection.Benchmarks/CSharpEssentials.DependencyInjection.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/di"
        ALL_FRAMEWORKS=("net8.0" "net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*Registration*" "*Decoration*" "*Resolve*")
        ;;
    mediator)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Mediator.Benchmarks/CSharpEssentials.Mediator.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/mediator"
        ALL_FRAMEWORKS=("net8.0" "net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*.SendBenchmarks.*" "*.PublishBenchmarks.*" "*.StartupBenchmarks.*" "*.ColdStartupBenchmarks.*" "*.BehaviorBenchmarks.*")
        ;;
    enums)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Enums.Comparison.Benchmarks/CSharpEssentials.Enums.Comparison.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/enums"
        ALL_FRAMEWORKS=("net8.0" "net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*.ToStringComparison.*" "*.ParseComparison.*" "*.IsDefinedComparison.*" "*.GetValuesComparison.*" "*.FlagsComparison.*" "*.DescriptionComparison.*")
        ;;
    any)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Any.Comparison.Benchmarks/CSharpEssentials.Any.Comparison.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/any"
        ALL_FRAMEWORKS=("net8.0" "net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*.ConstructComparison.*" "*.MatchComparison.*" "*.TryPickComparison.*" "*.PartitionComparison.*" "*.JsonComparison.*" "*.ToStringComparison.*" "*.TheseMatchComparison.*" "*.ThesePipelineComparison.*" "*.ThesePartitionComparison.*" "*.TheseJsonComparison.*")
        ;;
    json)
        PROJECT="$SCRIPT_DIR/CSharpEssentials.Json.Comparison.Benchmarks/CSharpEssentials.Json.Comparison.Benchmarks.csproj"
        RESULTS_DIR="$SCRIPT_DIR/results/json"
        ALL_FRAMEWORKS=("net8.0" "net9.0" "net10.0" "net11.0")
        QUICK_PATTERNS=("*.EnumComparison.*" "*.EnumFlagsComparison.*" "*.PolymorphicComparison.*" "*.DateTimeComparison.*" "*.CaseConversionComparison.*" "*.GuiderComparison.*" "*.RandomItemsComparison.*" "*.WithCancellationComparison.*")
        ;;
    *)
        echo "Unknown --project '$SUITE' (expected: validation | results | idempotency | endpoints | results-comparison | di | mediator | enums | any | json)" >&2
        exit 1
        ;;
esac

if [[ "$QUICK" == "true" ]]; then
    # Quick mode: single TFM, representative scenarios, Short job (~3-5 min total)
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
