#!/usr/bin/env bash
set -euo pipefail

echo "Restoring solution..."
dotnet restore ./src/Eventity.sln

echo "Building solution (collecting diagnostics)..."
BUILD_OUT=$(mktemp)
dotnet build ./src/Eventity.sln --no-restore 2>&1 | tee "$BUILD_OUT"

# Fail if cyclomatic complexity (CA1502) or StyleCop errors found
if grep -E "CA1502|\bSA[0-9]{4}\b" "$BUILD_OUT" >/dev/null; then
	echo "Static analysis failures detected (CA1502 or StyleCop)." >&2
	grep -E "CA1502|\bSA[0-9]{4}\b" "$BUILD_OUT" >&2 || true
	rm "$BUILD_OUT"
	exit 1
fi

echo "Static analysis: no CA1502/StyleCop violations found in build output."
rm "$BUILD_OUT"

# Run Halstead metrics scan (informational, fails only on extremely large values)
if command -v python3 >/dev/null 2>&1; then
	echo "Running Halstead metrics scan..."
	python3 scripts/halstead.py
else
	echo "python3 not found; skipping Halstead scan"
fi
