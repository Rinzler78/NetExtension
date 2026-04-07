#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

IMAGE_NAME="${IMAGE_NAME:-netextension-build}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
SHELL_PATH="${SHELL_PATH:-/bin/sh}"
EXTRA_ARGS=()

usage() {
    cat <<'EOF'
Usage: ./scripts/docker/run.sh [options] [-- extra docker args]

Options:
  --name <name>    Image name (default: netextension-build)
  --tag <tag>      Image tag (default: latest)
  --shell <path>   Command or shell to execute (default: /bin/sh)
  -h, --help       Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --name) IMAGE_NAME="$2"; shift 2 ;;
        --tag) IMAGE_TAG="$2"; shift 2 ;;
        --shell) SHELL_PATH="$2"; shift 2 ;;
        --) shift; EXTRA_ARGS+=("$@"); break ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

require_docker
if ! docker image inspect "${IMAGE_NAME}:${IMAGE_TAG}" >/dev/null 2>&1; then
    bash "${SCRIPT_DIR}/build.sh" --name "${IMAGE_NAME}" --tag "${IMAGE_TAG}"
fi

DOCKER_ARGS=(--rm -it)
if [[ ${#EXTRA_ARGS[@]} -gt 0 ]]; then
    DOCKER_ARGS+=("${EXTRA_ARGS[@]}")
fi
DOCKER_ARGS+=("${IMAGE_NAME}:${IMAGE_TAG}" "${SHELL_PATH}")

run_cmd docker run "${DOCKER_ARGS[@]}"
