#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

CONFIGURATION="${CONFIGURATION:-Release}"
RESULTS_DIR="${RESULTS_DIR:-${REPO_ROOT}/artifacts/test-results/local}"
NO_BUILD=false
COLLECT_COVERAGE=true
TEST_SUITE="all"

usage() {
    cat <<'EOF'
Usage: ./scripts/local/test.sh [options]

Options:
  --configuration <name>  Test configuration (default: Release)
  --results <dir>         Results directory (default: ./artifacts/test-results/local)
  --suite <name>          Test suite: all|unit|integration|e2e (default: all)
  --no-build              Skip build during dotnet test
  --no-coverage           Disable coverage collection
  -h, --help              Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --configuration) CONFIGURATION="$2"; shift 2 ;;
        --results) RESULTS_DIR="$2"; shift 2 ;;
        --suite) TEST_SUITE="$2"; shift 2 ;;
        --no-build) NO_BUILD=true; shift ;;
        --no-coverage) COLLECT_COVERAGE=false; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

case "${TEST_SUITE}" in
    all|unit|integration|e2e) ;;
    *) echo "Error: Invalid suite '${TEST_SUITE}'. Expected one of: all, unit, integration, e2e" >&2; exit 1 ;;
esac

require_command dotnet
rm -rf "${RESULTS_DIR}"
ensure_dir "${RESULTS_DIR}"
TEST_PROJECT="${REPO_ROOT}/src/Rinzler78.NetExtension.Tests/Rinzler78.NetExtension.Tests.csproj"
ARGS=(
    "${TEST_PROJECT}"
    --configuration "${CONFIGURATION}"
    --verbosity normal
    --logger "trx;LogFileName=netextension-tests.trx"
    --results-directory "${RESULTS_DIR}"
)

if [[ "${NO_BUILD}" == true ]]; then
    ensure_project_assets "${TEST_PROJECT}"

    # Work around a tooling issue observed with `dotnet test --no-build`
    # on this environment where VSTest receives an invalid DLL argument.
    # `--no-restore` preserves the fast path while keeping execution reliable.
    ARGS+=(--no-restore)
fi

if [[ "${COLLECT_COVERAGE}" == true ]]; then
    ARGS+=("--collect:XPlat Code Coverage")
fi

case "${TEST_SUITE}" in
    all)
        ;;
    unit)
        ARGS+=(--filter "Category=Unit")
        ;;
    integration)
        ARGS+=(--filter "Category=Integration")
        ;;
    e2e)
        ARGS+=(--filter "Category=E2E")
        ;;
esac

print_header "NetExtension Local Test"
run_cmd dotnet test "${ARGS[@]}"
