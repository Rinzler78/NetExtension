#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

IMAGE_NAME="${IMAGE_NAME:-netextension-build}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
BUILD_CONFIGURATION="${BUILD_CONFIGURATION:-Release}"
NO_CACHE=""

usage() {
    cat <<'EOF'
Usage: ./scripts/docker/build.sh [options]

Options:
  --name <name>           Image name (default: netextension-build)
  --tag <tag>             Image tag (default: latest)
  --configuration <name>  Build configuration (default: Release)
  --no-cache              Disable Docker layer cache
  -h, --help              Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --name) IMAGE_NAME="$2"; shift 2 ;;
        --tag) IMAGE_TAG="$2"; shift 2 ;;
        --configuration) BUILD_CONFIGURATION="$2"; shift 2 ;;
        --no-cache) NO_CACHE="--no-cache"; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

require_docker
run_cmd docker build ${NO_CACHE} --target build --build-arg BUILD_CONFIGURATION="${BUILD_CONFIGURATION}" -t "${IMAGE_NAME}:${IMAGE_TAG}" -f "${REPO_ROOT}/docker/netextension/Dockerfile" "${REPO_ROOT}"
