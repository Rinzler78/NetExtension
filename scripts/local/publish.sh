#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

CONFIGURATION="${CONFIGURATION:-Release}"
OUTPUT_DIR="${OUTPUT_DIR:-${REPO_ROOT}/artifacts/packages/local}"
NO_BUILD=false

usage() {
    cat <<'EOF'
Usage: ./scripts/local/publish.sh [options]

Options:
  --configuration <name>  Pack configuration (default: Release)
  --output <dir>          Output directory (default: ./artifacts/packages/local)
  --no-build              Skip build during dotnet pack
  -h, --help              Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --configuration) CONFIGURATION="$2"; shift 2 ;;
        --output) OUTPUT_DIR="$2"; shift 2 ;;
        --no-build) NO_BUILD=true; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

require_command dotnet
ensure_dir "${OUTPUT_DIR}"
PACKAGE_PROJECT="${REPO_ROOT}/src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj"
ARGS=(
    "${PACKAGE_PROJECT}"
    --configuration "${CONFIGURATION}"
    --output "${OUTPUT_DIR}"
)

if [[ "${NO_BUILD}" == true ]]; then
    ensure_project_assets "${PACKAGE_PROJECT}"
    ARGS+=(--no-build)
fi

print_header "NetExtension Local Publish"
run_cmd dotnet pack "${ARGS[@]}"
