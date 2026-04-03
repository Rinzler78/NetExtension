#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/lib/common.sh"

DEFAULT_INPUT="${REPO_ROOT}/artifacts/test-results"
COMMAND="${1:-summary}"
REPORTGENERATOR_PATH="${REPO_ROOT}/.tools/reportgenerator"

if [[ $# -gt 0 ]]; then
    shift
fi

if [[ "${COMMAND}" == "--help" || "${COMMAND}" == "-h" ]]; then
    run_cmd perl "${REPO_ROOT}/scripts/lib/coverage-report.pl" --help
    exit 0
fi

CURRENT_ARGS=" $* "
ARGS=("${COMMAND}")
if [[ "${CURRENT_ARGS}" != *" --input "* ]]; then
    ARGS+=(--input "${DEFAULT_INPUT}")
fi

if [[ "${COMMAND}" == "gate" ]]; then
    if [[ "${CURRENT_ARGS}" != *" --config "* ]]; then
        ARGS+=(--config "${REPO_ROOT}/docs/testing-quality/quality-gates.json")
    fi

    if [[ "${CURRENT_ARGS}" != *" --baseline "* ]]; then
        ARGS+=(--baseline "${REPO_ROOT}/docs/testing-quality/coverage-baseline.json")
    fi
fi

ARGS+=("$@")

INPUT_PATH="${DEFAULT_INPUT}"
for ((index=0; index<${#ARGS[@]}; index++)); do
    if [[ "${ARGS[index]}" == "--input" && $((index + 1)) -lt ${#ARGS[@]} ]]; then
        INPUT_PATH="${ARGS[index + 1]}"
        break
    fi
done

require_command dotnet
ensure_dir "${REPO_ROOT}/.tools"
if [[ ! -x "${REPORTGENERATOR_PATH}" ]]; then
    run_cmd dotnet tool install dotnet-reportgenerator-globaltool --tool-path "${REPO_ROOT}/.tools"
fi

REPORTS_ARG="${INPUT_PATH}"
if [[ -d "${INPUT_PATH}" ]]; then
    REPORTS_ARG="${INPUT_PATH}/**/coverage.cobertura.xml"
fi

TEMP_DIR="$(mktemp -d)"
trap 'rm -rf "${TEMP_DIR}"' EXIT
run_cmd "${REPORTGENERATOR_PATH}" -reports:"${REPORTS_ARG}" -targetdir:"${TEMP_DIR}" -reporttypes:"JsonSummary" >/dev/null

run_cmd perl "${REPO_ROOT}/scripts/lib/coverage-report.pl" "${COMMAND}" "${ARGS[@]:1}"
