#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

CONFIGURATION="${CONFIGURATION:-Release}"

usage() {
    cat <<'EOF'
Usage: ./scripts/local/build.sh [options]

Options:
  --configuration <name>  Build configuration (default: Release)
  -h, --help              Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --configuration) CONFIGURATION="$2"; shift 2 ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

require_command dotnet
ensure_hooks
print_header "NetExtension Local Build"
run_cmd dotnet restore "${REPO_ROOT}/src/Rinzler78.NetExtension.sln"
run_cmd dotnet build "${REPO_ROOT}/src/Rinzler78.NetExtension.sln" --configuration "${CONFIGURATION}" --no-restore
