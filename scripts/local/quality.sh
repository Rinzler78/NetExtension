#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

CONFIGURATION="${CONFIGURATION:-Release}"
RESULTS_DIR="${RESULTS_DIR:-${REPO_ROOT}/artifacts/test-results/quality-local}"
COVERAGE_POLICY="${COVERAGE_POLICY:-${REPO_ROOT}/docs/testing-quality/quality-gates.json}"
COVERAGE_BASELINE="${COVERAGE_BASELINE:-${REPO_ROOT}/docs/testing-quality/coverage-baseline.json}"
NO_BUILD=false

usage() {
    cat <<'EOF'
Usage: ./scripts/local/quality.sh [options]

Options:
  --configuration <name>  Validation configuration (default: Release)
  --results <dir>         Results directory (default: ./artifacts/test-results/quality-local)
  --policy <path>         Coverage policy JSON
  --baseline <path>       Coverage baseline JSON
  --no-build              Skip build before validation
  -h, --help              Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --configuration) CONFIGURATION="$2"; shift 2 ;;
        --results) RESULTS_DIR="$2"; shift 2 ;;
        --policy) COVERAGE_POLICY="$2"; shift 2 ;;
        --baseline) COVERAGE_BASELINE="$2"; shift 2 ;;
        --no-build) NO_BUILD=true; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

ensure_hooks
require_file "${COVERAGE_POLICY}"
require_file "${COVERAGE_BASELINE}"

ensure_dir "${RESULTS_DIR}"

if [[ "${NO_BUILD}" == false ]]; then
    run_cmd "${REPO_ROOT}/scripts/local/build.sh" --configuration "${CONFIGURATION}"
fi

run_cmd "${REPO_ROOT}/scripts/local/test.sh" --configuration "${CONFIGURATION}" --no-build --suite unit --no-coverage --results "${RESULTS_DIR}/unit"
run_cmd "${REPO_ROOT}/scripts/local/test.sh" --configuration "${CONFIGURATION}" --no-build --suite integration --no-coverage --results "${RESULTS_DIR}/integration"
run_cmd "${REPO_ROOT}/scripts/local/test.sh" --configuration "${CONFIGURATION}" --no-build --suite e2e --no-coverage --results "${RESULTS_DIR}/e2e"
run_cmd "${REPO_ROOT}/scripts/local/test.sh" --configuration "${CONFIGURATION}" --no-build --suite all --results "${RESULTS_DIR}/coverage"

run_cmd "${REPO_ROOT}/scripts/coverage-report.sh" summary \
    --input "${RESULTS_DIR}/coverage" \
    --json-out "${RESULTS_DIR}/coverage-summary.json" \
    --markdown-out "${RESULTS_DIR}/coverage-summary.md"

run_cmd "${REPO_ROOT}/scripts/coverage-report.sh" gate \
    --input "${RESULTS_DIR}/coverage" \
    --config "${COVERAGE_POLICY}" \
    --baseline "${COVERAGE_BASELINE}" \
    --json-out "${RESULTS_DIR}/coverage-gate.json" \
    --markdown-out "${RESULTS_DIR}/coverage-gate.md"
