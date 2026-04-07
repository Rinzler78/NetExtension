#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

IMAGE_NAME="${IMAGE_NAME:-netextension-test}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
RESULTS_DIR="${RESULTS_DIR:-${REPO_ROOT}/artifacts/test-results/docker}"
NO_CACHE=""
KEEP_IMAGE=false
TEST_SUITE="all"
COLLECT_COVERAGE=true

usage() {
    cat <<'EOF'
Usage: ./scripts/docker/test.sh [options]

Options:
  --name <name>    Image name (default: netextension-test)
  --tag <tag>      Image tag (default: latest)
  --results <dir>  Results directory (default: ./artifacts/test-results/docker)
  --suite <name>   Test suite: all|unit|integration|e2e (default: all)
  --no-cache       Disable Docker layer cache
  --no-coverage    Disable coverage collection
  --keep           Keep the image after the run
  -h, --help       Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --name) IMAGE_NAME="$2"; shift 2 ;;
        --tag) IMAGE_TAG="$2"; shift 2 ;;
        --results) RESULTS_DIR="$2"; shift 2 ;;
        --suite) TEST_SUITE="$2"; shift 2 ;;
        --no-cache) NO_CACHE="--no-cache"; shift ;;
        --no-coverage) COLLECT_COVERAGE=false; shift ;;
        --keep) KEEP_IMAGE=true; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

case "${TEST_SUITE}" in
    all|unit|integration|e2e) ;;
    *) echo "Error: Invalid suite '${TEST_SUITE}'. Expected one of: all, unit, integration, e2e" >&2; exit 1 ;;
esac

if [[ "${RESULTS_DIR}" != /* ]]; then
    RESULTS_DIR="${PWD}/${RESULTS_DIR}"
fi

require_docker
rm -rf "${RESULTS_DIR}"
ensure_dir "${RESULTS_DIR}"
run_cmd docker build ${NO_CACHE} --target test -t "${IMAGE_NAME}:${IMAGE_TAG}" -f "${REPO_ROOT}/docker/netextension/Dockerfile" "${REPO_ROOT}"

TEST_ARGS=(
    dotnet test src/Rinzler78.NetExtension.Tests/Rinzler78.NetExtension.Tests.csproj
    --no-build
    --configuration Release
    --logger "trx;LogFileName=/results/netextension-${TEST_SUITE}-tests.trx"
    --results-directory /results
    --verbosity normal
)

if [[ "${COLLECT_COVERAGE}" == true ]]; then
    TEST_ARGS+=("--collect:XPlat Code Coverage")
fi

case "${TEST_SUITE}" in
    all)
        ;;
    unit)
        TEST_ARGS+=(--filter "Category=Unit")
        ;;
    integration)
        TEST_ARGS+=(--filter "Category=Integration")
        ;;
    e2e)
        TEST_ARGS+=(--filter "Category=E2E")
        ;;
esac

printf -v TEST_COMMAND '%q ' "${TEST_ARGS[@]}"

CONTAINER_NAME="netextension-test-${TEST_SUITE}-$$-$(date +%s)"

set +e
run_cmd docker create --name "${CONTAINER_NAME}" "${IMAGE_NAME}:${IMAGE_TAG}" "${TEST_COMMAND}"
CREATE_EXIT_CODE=$?
if [[ ${CREATE_EXIT_CODE} -ne 0 ]]; then
    TEST_EXIT_CODE=${CREATE_EXIT_CODE}
else
    run_cmd docker start -a "${CONTAINER_NAME}"
    TEST_EXIT_CODE=$?
    docker cp "${CONTAINER_NAME}:/results/." "${RESULTS_DIR}" >/dev/null 2>&1 || true
    docker rm -f "${CONTAINER_NAME}" >/dev/null 2>&1 || true
fi
set -e
if [[ "${KEEP_IMAGE}" == false ]]; then
    docker rmi "${IMAGE_NAME}:${IMAGE_TAG}" >/dev/null 2>&1 || true
fi
exit ${TEST_EXIT_CODE}
