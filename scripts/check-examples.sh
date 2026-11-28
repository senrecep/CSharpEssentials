#!/usr/bin/env bash
# Build every project under examples/, run the console ones and test the test projects.
# Web apps and examples that need a cloud account are only built. The Redis and
# end-to-end idempotency examples need Docker and have their own CI steps, so they are skipped here.
# Usage: scripts/check-examples.sh [Configuration]
set -uo pipefail
cd "$(dirname "$0")/.."
config="${1:-Release}"
failed=()
for proj in examples/*/*.csproj; do
  name="$(basename "$proj" .csproj)"
  case "$name" in
    Examples.Idempotency.Redis|Examples.Idempotency.EndToEnd) continue ;;
  esac
  if grep -q 'xunit' "$proj"; then
    mode=test
  elif grep -q 'Microsoft.NET.Sdk.Web' "$proj" || grep -q '// build-only' "$(dirname "$proj")/Program.cs" 2>/dev/null || [ "$name" = Examples.GcpSecretManager ]; then
    mode=build
  else
    mode=run
  fi
  echo "::group::$name ($mode)"
  case "$mode" in
    test)  dotnet test "$proj" -c "$config" -v:q -nologo ;;
    build) dotnet build "$proj" -c "$config" -v:q -nologo ;;
    run)   dotnet build "$proj" -c "$config" -v:q -nologo && timeout 120 dotnet run --project "$proj" -c "$config" --no-build > /dev/null ;;
  esac
  rc=$?
  echo "::endgroup::"
  [ $rc -eq 0 ] || failed+=("$name ($mode, exit $rc)")
done
if [ ${#failed[@]} -gt 0 ]; then
  printf 'FAILED: %s\n' "${failed[@]}"
  exit 1
fi
echo "All examples OK"
