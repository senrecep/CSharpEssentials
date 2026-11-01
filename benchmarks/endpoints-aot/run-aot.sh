#!/usr/bin/env bash
# Native AOT report for the endpoint samples in this folder.
# For each sample: Native AOT publish (IL warnings, success, binary size), framework-dependent JIT publish,
# then time from process start to the first 200 on GET /users/1 (median of RUNS, after one discarded warm-up
# run that also checks the other two routes).
# Usage: run-aot.sh [Variant ...]   (default: all; a subset rewrites the report with only those rows).  Env: RUNS (default 5), STARTUP_TIMEOUT_S (default 20).
# Output: benchmarks/results/endpoints/aot/aot-report.md, raw logs in .../aot/logs/ (git-ignored by the repo's [Ll]ogs/ rule).
set -euo pipefail

HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
OUT="$HERE/../results/endpoints/aot"
LOGS="$OUT/logs"
RUNS=${RUNS:-5}
STARTUP_TIMEOUT_S=${STARTUP_TIMEOUT_S:-20}
ALL=(PlainAot CseGeneratedAot CseReflectionAot CarterAot FastEndpointsAot FastEndpointsSourceGenAot)
VARIANTS=("$@")
((${#VARIANTS[@]})) || VARIANTS=("${ALL[@]}")

mkdir -p "$LOGS"
ulimit -c 0 # crashing samples are expected; no core dumps
LOAD0=$(cut -d" " -f1-3 /proc/loadavg)
OUT=$(cd "$OUT" && pwd)
WORK=$(mktemp -d)
mkfifo "$WORK/tick"
exec 9<>"$WORK/tick" # read -t on this fd is a fork-free sleep
SERVER_PID=

cleanup() {
    [[ -n $SERVER_PID ]] && kill "$SERVER_PID" 2>/dev/null || true
    for v in "${VARIANTS[@]}"; do rm -rf "${HERE:?}/$v/bin" "${HERE:?}/$v/obj"; done
    rm -rf "$WORK"
}
trap cleanup EXIT

free_port() {
    local p
    while :; do
        p=$((20000 + RANDOM % 20000))
        ss -Htln "sport = :$p" | grep -q . || { echo "$p"; return; }
    done
}

# http METHOD PATH [JSON]: sets STATUS (empty when nothing answers). /dev/tcp keeps the poll loop fork-free.
http() {
    STATUS=
    { exec 3<>"/dev/tcp/127.0.0.1/$PORT"; } 2>/dev/null || return 0
    if [[ -n ${3:-} ]]; then
        printf '%s %s HTTP/1.1\r\nHost: localhost\r\nContent-Type: application/json\r\nContent-Length: %d\r\nConnection: close\r\n\r\n%s' "$1" "$2" "${#3}" "$3" >&3
    else
        printf '%s %s HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\nConnection: close\r\n\r\n' "$1" "$2" >&3
    fi
    local _proto _rest
    IFS=' ' read -r -t 5 _proto STATUS _rest <&3 || STATUS=
    exec 3<&- 3>&-
}

now_us() { local t=${EPOCHREALTIME/[.,]/}; echo "$t"; }

# start_and_time BINARY LOGFILE: sets ST to ms until the first 200 on GET /users/1, or "fail:<reason>". Leaves the server running.
start_and_time() {
    local bin=$1 log=$2 t0 t1 deadline
    PORT=$(free_port)
    t0=${EPOCHREALTIME/[.,]/}
    ASPNETCORE_URLS="http://127.0.0.1:$PORT" "$bin" >"$log" 2>&1 &
    SERVER_PID=$!
    deadline=$((t0 + STARTUP_TIMEOUT_S * 1000000))
    while :; do
        http GET /users/1
        if [[ $STATUS == 200 ]]; then
            t1=${EPOCHREALTIME/[.,]/}
            ST=$(((t1 - t0 + 500) / 1000))
            return
        fi
        if ! kill -0 "$SERVER_PID" 2>/dev/null; then
            ST="fail:exited: $(grep -m1 -E 'Exception' "$log" | sed 's/^ *//; s/Unhandled exception. //; s/|/\\|/g' | cut -c1-160 || true)"
            return
        fi
        if [[ -n $STATUS ]] || (($(now_us) > deadline)); then
            if [[ -n $STATUS ]]; then ST="fail:GET=$STATUS"; else ST="fail:timeout"; fi
            return
        fi
        read -r -t 0.002 -u 9 _ || true
    done
}

stop_server() {
    [[ -n $SERVER_PID ]] || return 0
    kill "$SERVER_PID" 2>/dev/null || true
    wait "$SERVER_PID" 2>/dev/null || true
    SERVER_PID=
}

# route_check: the three routes must behave identically in every variant.
route_check() {
    local post_ok post_bad del
    http POST /users '{"name":"Ada","email":"ada@example.com","age":30}'; post_ok=$STATUS
    http POST /users '{"name":"","email":"not-an-email","age":5}'; post_bad=$STATUS
    http DELETE /users/1; del=$STATUS
    if [[ $post_ok =~ ^20[01]$ && $post_bad == 400 && $del == 204 ]]; then
        echo "ok (POST $post_ok/400, DELETE 204)"
    else
        echo "MISMATCH (POST valid=${post_ok:-none}, invalid=${post_bad:-none}, DELETE=${del:-none})"
    fi
}

median() { printf '%s\n' "$@" | sort -n | awk '{a[NR]=$1} END {print (NR % 2) ? a[(NR + 1) / 2] : int((a[NR / 2] + a[NR / 2 + 1]) / 2)}'; }

# measure VARIANT MODE BINARY: sets M_RESULT (median ms or failure) and M_CHECK.
measure() {
    local v=$1 mode=$2 bin=$3 r i times=()
    M_CHECK="-"
    start_and_time "$bin" "$LOGS/$v.$mode.run.log"; r=$ST
    if [[ $r == fail:* ]]; then stop_server; M_RESULT=${r#fail:}; return; fi
    M_CHECK=$(route_check)
    stop_server
    for ((i = 1; i <= RUNS; i++)); do
        start_and_time "$bin" "$WORK/run.log"; r=$ST
        stop_server
        if [[ $r == fail:* ]]; then cp "$WORK/run.log" "$LOGS/$v.$mode.run.log"; M_RESULT="run $i ${r#fail:}"; return; fi
        times+=("$r")
    done
    M_RESULT=$(median "${times[@]}")
    echo "  $mode startup runs (ms): ${times[*]}" >&2
}

clean_obj() { rm -rf "$HERE/$1/bin" "$HERE/$1/obj"; }

ROWS=()
DETAILS=()

for v in "${VARIANTS[@]}"; do
    echo "== $v" >&2
    proj="$HERE/$v/$v.csproj"
    alog="$LOGS/$v.aot-publish.log"
    jlog="$LOGS/$v.jit-publish.log"

    # (a) Native AOT. Clean first: an incremental publish skips CoreCompile and drops the analyzer warnings.
    # TrimmerSingleWarn=false lists member-level ILC warnings instead of one IL2104/IL3053 line per package assembly.
    # PublishAot comes from the csproj: as a global -p it would also reach the netstandard2.0 generator
    # project references of the CSE samples and fail them with NETSDK1207.
    clean_obj "$v"
    aot_ok=yes
    dotnet publish "$proj" -c Release -r linux-x64 -p:TreatWarningsAsErrors=false \
        -p:TrimmerSingleWarn=false -o "$WORK/$v/aot" --tl:off -clp:NoSummary >"$alog" 2>&1 || aot_ok=no
    # Distinct warnings: drop the trailing [project] tag and the repo path prefix, then dedupe.
    grep -E 'warning IL[0-9]+' "$alog" | sed -E 's/ \[[^]]*\]$//; s#^ *##; s#'"${HERE%/benchmarks/*}"'/##' | sort -u >"$WORK/$v.warn" || true
    n_total=$(wc -l <"$WORK/$v.warn")
    # Roslyn analyzer lines carry file(line,col); ILC lines carry file(line) or no location.
    n_an=$(grep -cE '\([0-9]+,[0-9]+\): warning IL' "$WORK/$v.warn" || true)
    n_ilc=$((n_total - n_an))
    codes=$(grep -oE 'warning IL[0-9]+' "$WORK/$v.warn" | sed 's/warning //' | sort | uniq -c | awk '{printf "%s%s x%s", (NR > 1 ? ", " : ""), $2, $1}' || true)
    size="-"
    if [[ $aot_ok == yes && -f $WORK/$v/aot/$v ]]; then
        size=$(awk -v b="$(stat -c %s "$WORK/$v/aot/$v")" 'BEGIN {printf "%.1f MB", b / 1048576}')
    else
        aot_ok=no
    fi
    clean_obj "$v"

    # (b) framework-dependent JIT publish (the csproj sets PublishAot=true, so turn it off explicitly).
    jit_ok=yes
    dotnet publish "$proj" -c Release -r linux-x64 --self-contained false -p:PublishAot=false \
        -p:TreatWarningsAsErrors=false -o "$WORK/$v/jit" --tl:off -clp:NoSummary >"$jlog" 2>&1 || jit_ok=no
    clean_obj "$v"

    # (c) startup
    jit_ms="publish failed"; jit_chk="-"; aot_ms="publish failed"; aot_chk="-"
    if [[ $jit_ok == yes ]]; then measure "$v" jit "$WORK/$v/jit/$v"; jit_ms=$M_RESULT; jit_chk=$M_CHECK; fi
    if [[ $aot_ok == yes ]]; then measure "$v" aot "$WORK/$v/aot/$v"; aot_ms=$M_RESULT; aot_chk=$M_CHECK; fi

    # A binary that does not serve all 3 routes had application code trimmed away: flag its size and startup.
    if [[ $aot_ok == yes && $aot_chk != ok* ]]; then
        size="$size [1]"
        [[ $aot_ms =~ ^[0-9]+$ ]] && aot_ms="$aot_ms [1]"
    fi
    ROWS+=("| $v | $aot_ok | $n_total ($n_an analyzer / $n_ilc ILC) | ${codes:--} | $size | $jit_ms | $aot_ms | $jit_chk | $aot_chk |")
    detail="### $v"$'\n\n'
    if ((n_total == 0)); then
        detail+="No IL warnings."$'\n'
    else
        detail+='```text'$'\n'"$(cut -c1-400 "$WORK/$v.warn")"$'\n''```'$'\n'
    fi
    DETAILS+=("$detail")
    rm -rf "${WORK:?}/$v"
done

{
    echo "# Endpoints Native AOT report"
    echo
    echo "Generated by \`benchmarks/endpoints-aot/run-aot.sh\` on $(date -u +%Y-%m-%dT%H:%MZ)."
    echo "SDK $(dotnet --version), target net10.0, linux-x64, kernel $(uname -r), $(nproc) logical CPUs, load average at start: $LOAD0."
    echo
    echo "- **AOT publish**: \`dotnet publish -c Release -r linux-x64 -p:TreatWarningsAsErrors=false -p:TrimmerSingleWarn=false\` from a clean obj; every sample sets \`PublishAot=true\` in its csproj (as a global \`-p\` it would also reach the CSE generator project references and fail them with NETSDK1207)."
    echo "  TreatWarningsAsErrors=false is passed on the command line only; nothing is suppressed in the projects."
    echo "  TrimmerSingleWarn=false makes ILC list each warning with its member instead of one IL2104/IL3053 line per package assembly, so counts are higher than a default publish shows."
    echo "- **IL warnings**: distinct \`warning IL\` lines. *analyzer* = Roslyn trim/AOT analyzers at compile time (call sites in the sample); *ILC* = whole-program trim/AOT analysis during native compilation (inside libraries)."
    echo "- **Startup**: ms from process start to the first 200 on \`GET /users/1\`, median of $RUNS runs after one discarded warm-up run; polled every ~2 ms over a raw socket. JIT = framework-dependent publish with PublishAot=false (so no Request Delegate Generator)."
    echo "- **Route check** (warm-up run): POST valid -> 201, POST invalid -> 400, DELETE -> 204. Validation engines differ: DataAnnotations + \`AddValidation()\` for Plain/CSE, FluentValidation for Carter and FastEndpoints."
    echo "- **JSON contexts**: Native AOT turns reflection-based JSON off. Plain, CSE and Carter list \`HttpValidationProblemDetails\` in their \`JsonSerializerContext\` because the 400 body is written with the app JSON options; without it invalid input returns 500. The FastEndpoints samples list \`ErrorResponse\` because a per-endpoint \`SerializerContext<T>()\` replaces the default resolver for that endpoint, failure body included."
    echo "- **FastEndpoints**: JSON uses the documented manual per-endpoint context (model-binding#source-generators) rather than \`GenerateSerializerContexts\`, which installs a local dotnet tool from the network on every fresh build. Request DTOs use \`{ get; set; }\`, because FastEndpoints.Generator emits no setter for init-only properties and the binder would fall back to runtime-compiled setters. FastEndpointsSourceGenAot registers the generated reflection data (configuration-settings#source-generated-reflection)."
    echo "- **Carter** documents no Native AOT approach, so CarterAot uses the plain minimal-API \`JsonSerializerContext\`; it gets an \`Assembly[]\` catalog because a single \`Assembly\` binds to the constructor that walks deps.json. Invalid input answers 400 (the Carter sample answers 422) so every variant returns the same status."
    echo "- Startup numbers are machine-noise sensitive; regenerate on a quiet machine before quoting them."
    echo
    echo "| Variant | AOT publish | IL warnings (distinct) | Warning codes | Native binary | JIT startup (ms) | AOT startup (ms) | Route check JIT | Route check AOT |"
    echo "|---|---|---|---|---|---|---|---|---|"
    printf '%s\n' "${ROWS[@]}"
    echo
    echo "[1] The AOT binary does not serve all three routes (trimming removed application code the library reaches only by reflection), so its size and startup are not comparable with the working variants."
    echo
    echo "## Distinct warnings per variant"
    echo
    printf '%s\n' "${DETAILS[@]}"
} >"$OUT/aot-report.md"

echo "Report: $OUT/aot-report.md" >&2
