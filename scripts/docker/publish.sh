#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

IMAGE_NAME="${IMAGE_NAME:-netextension-pack}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
OUTPUT_DIR="${OUTPUT_DIR:-${REPO_ROOT}/artifacts/packages/docker}"
NO_CACHE=""
KEEP_IMAGE=false

usage() {
    cat <<'EOF'
Usage: ./scripts/docker/publish.sh [options]

Options:
  --name <name>    Image name (default: netextension-pack)
  --tag <tag>      Image tag (default: latest)
  --output <dir>   Output directory (default: ./artifacts/packages/docker)
  --no-cache       Disable Docker layer cache
  --keep           Keep the image after export
  -h, --help       Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --name) IMAGE_NAME="$2"; shift 2 ;;
        --tag) IMAGE_TAG="$2"; shift 2 ;;
        --output) OUTPUT_DIR="$2"; shift 2 ;;
        --no-cache) NO_CACHE="--no-cache"; shift ;;
        --keep) KEEP_IMAGE=true; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

require_docker
ensure_dir "${OUTPUT_DIR}"
run_cmd docker build ${NO_CACHE} --target pack -t "${IMAGE_NAME}:${IMAGE_TAG}" -f "${REPO_ROOT}/docker/netextension/Dockerfile" "${REPO_ROOT}"
CONTAINER_ID="$(docker create "${IMAGE_NAME}:${IMAGE_TAG}")"
trap 'docker rm -f "${CONTAINER_ID}" >/dev/null 2>&1 || true' EXIT
run_cmd docker cp "${CONTAINER_ID}:/artifacts/." "${OUTPUT_DIR}"
if [[ "${KEEP_IMAGE}" == false ]]; then
    docker rmi "${IMAGE_NAME}:${IMAGE_TAG}" >/dev/null 2>&1 || true
fi
