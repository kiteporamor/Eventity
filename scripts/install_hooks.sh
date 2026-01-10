#!/usr/bin/env bash
set -euo pipefail

echo "Setting repository git hooks path to .githooks"
git config core.hooksPath .githooks
echo "Done. Pre-commit hook will run checks before commits. Use --no-verify to bypass."
