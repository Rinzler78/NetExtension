#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/lib/common.sh"

ARGS=()

if [[ "${1:-}" == "--help" || "${1:-}" == "-h" ]]; then
    run_cmd perl "${REPO_ROOT}/scripts/lib/changelog-extract.pl" --help
    exit 0
fi

CURRENT_ARGS=" $* "

if [[ "${CURRENT_ARGS}" != *" --file "* ]]; then
    ARGS+=(--file "${REPO_ROOT}/CHANGELOG.md")
fi

if [[ "${CURRENT_ARGS}" != *" --version "* ]]; then
    ARGS+=(--version "${VERSION:-Unreleased}")
fi

ARGS+=("$@")
run_cmd perl "${REPO_ROOT}/scripts/lib/changelog-extract.pl" "${ARGS[@]}"
