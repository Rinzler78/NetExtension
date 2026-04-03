#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

CONFIGURATION="${CONFIGURATION:-Release}"
FILTER="${FILTER:-}"

usage() {
    cat <<'EOF'
Usage: ./scripts/local/run.sh [options]

Run the verification suite for the library.

Options:
  --configuration <name>  Run configuration (default: Release)
  --filter <expr>         dotnet test filter
  -h, --help              Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --configuration) CONFIGURATION="$2"; shift 2 ;;
        --filter) FILTER="$2"; shift 2 ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Error: Unknown option: $1" >&2; exit 1 ;;
    esac
done

require_command dotnet
ARGS=(
    "${REPO_ROOT}/src/Rinzler78.NetExtension.Tests/Rinzler78.NetExtension.Tests.csproj"
    --configuration "${CONFIGURATION}"
    --verbosity normal
)
if [[ -n "${FILTER}" ]]; then
    ARGS+=(--filter "${FILTER}")
fi

print_header "NetExtension Local Run"
run_cmd dotnet test "${ARGS[@]}"
