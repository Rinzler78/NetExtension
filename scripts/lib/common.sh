#!/usr/bin/env bash

set -euo pipefail

COMMON_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${COMMON_DIR}/../.." && pwd)"

print_header() {
    local title="$1"
    printf '========================================\n'
    printf '%s\n' "$title"
    printf '========================================\n'
}

die() {
    echo "Error: $*" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || die "Required command not found: $1"
}

## ---------------------------------------------------------------------------
## Hooks
## ---------------------------------------------------------------------------

# Installe les hooks pre-commit si pre-commit est disponible et que les hooks
# ne sont pas encore installés dans ce dépôt/worktree.
# Silencieux si pre-commit n'est pas installé (pas de dépendance obligatoire).
ensure_hooks() {
    # Trouve la racine du dépôt courant (fonctionne aussi dans un worktree)
    local git_root
    git_root="$(git rev-parse --show-toplevel 2>/dev/null)" || return 0

    # Vérifie que pre-commit est disponible
    command -v pre-commit >/dev/null 2>&1 || return 0

    # Vérifie que .pre-commit-config.yaml existe dans ce dépôt
    [[ -f "${git_root}/.pre-commit-config.yaml" ]] || return 0

    # Identifie le fichier de hook réel (dans un worktree, .git est un fichier)
    local hooks_dir
    hooks_dir="$(git rev-parse --git-path hooks 2>/dev/null)" || return 0
    local hook_file="${hooks_dir}/pre-commit"

    # Installe seulement si le hook est absent ou n'est pas un hook pre-commit
    if [[ ! -f "${hook_file}" ]] || ! grep -q "pre-commit" "${hook_file}" 2>/dev/null; then
        echo "→ Installing pre-commit hooks in $(basename "${git_root}")…"
        (cd "${git_root}" && pre-commit install --install-hooks -t pre-commit -t commit-msg 2>/dev/null) || true
    fi
}

require_docker() {
    require_command docker
    docker info >/dev/null 2>&1 || die "Docker daemon is not available"
}

ensure_dir() {
    mkdir -p "$1"
}

require_file() {
    [[ -f "$1" ]] || die "Required file not found: $1"
}

ensure_project_assets() {
    local project_path="$1"
    local assets_path
    assets_path="$(dirname "${project_path}")/obj/project.assets.json"

    if [[ ! -f "${assets_path}" ]]; then
        echo "Missing assets file for $(basename "${project_path}"); restoring project"
        run_cmd dotnet restore "${project_path}"
    fi
}

run_cmd() {
    echo "+ $*"
    "$@"
}
