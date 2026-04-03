#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/lib/common.sh"

if [[ "${1:-}" == "--help" || "${1:-}" == "-h" ]]; then
    run_cmd perl "${REPO_ROOT}/scripts/lib/check-comments-language.pl" --help
    exit 0
fi

if [[ $# -eq 0 ]]; then
    FILES=()
    while IFS= read -r file; do
        FILES+=("${file}")
    done < <(find "${REPO_ROOT}/src" -name '*.cs' -type f | sort)
    run_cmd perl "${REPO_ROOT}/scripts/lib/check-comments-language.pl" "${FILES[@]}"
    exit 0
fi

run_cmd perl "${REPO_ROOT}/scripts/lib/check-comments-language.pl" "$@"
