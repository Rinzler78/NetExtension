#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

RESULTS_DIR="${RESULTS_DIR:-${REPO_ROOT}/artifacts/test-results/quality-docker}"
COVERAGE_POLICY="${COVERAGE_POLICY:-${REPO_ROOT}/docs/testing-quality/quality-gates.json}"
COVERAGE_BASELINE="${COVERAGE_BASELINE:-${REPO_ROOT}/docs/testing-quality/coverage-baseline.json}"

usage() {
    cat <<'EOF'
Usage: ./scripts/docker/quality.sh [options]

Options:
  --results <dir>    Results directory (default: ./artifacts/test-results/quality-docker)
  --policy <path>    Coverage policy JSON
  --baseline <path>  Coverage baseline JSON
  -h, --help         Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --results) RESULTS_DIR="$2"; shift 2 ;;
        --policy) COVERAGE_POLICY="$2"; shift 2 ;;
        --baseline) COVERAGE_BASELINE="$2"; shift 2 ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

require_docker
require_file "${COVERAGE_POLICY}"
require_file "${COVERAGE_BASELINE}"

ensure_dir "${RESULTS_DIR}"

run_cmd "${REPO_ROOT}/scripts/docker/test.sh" --suite unit --no-coverage --results "${RESULTS_DIR}/unit"
run_cmd "${REPO_ROOT}/scripts/docker/test.sh" --suite integration --no-coverage --results "${RESULTS_DIR}/integration"
run_cmd "${REPO_ROOT}/scripts/docker/test.sh" --suite e2e --no-coverage --results "${RESULTS_DIR}/e2e"
run_cmd "${REPO_ROOT}/scripts/docker/test.sh" --suite all --results "${RESULTS_DIR}/coverage"

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
