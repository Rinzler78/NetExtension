#!/usr/bin/env bash
# setup.sh — Initialise the local development environment.
# Run once after cloning the repository.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/../lib/common.sh"

print_header "NetExtension Setup"

# 1. Pre-commit hooks
if command -v pre-commit >/dev/null 2>&1; then
    echo "→ Installing pre-commit hooks..."
    pre-commit install -t pre-commit -t commit-msg
    echo "  ✓ Hooks installed"
else
    echo "  ⚠ pre-commit not found — install with: pip install pre-commit"
fi

# 2. NuGet restore
echo "→ Restoring NuGet packages..."
run_cmd dotnet restore "${REPO_ROOT}/src/Rinzler78.NetExtension.sln"
echo "  ✓ Packages restored"

echo ""
echo "Setup complete. Run ./scripts/local/build.sh to build."
