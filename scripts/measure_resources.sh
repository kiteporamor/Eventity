#!/usr/bin/env bash
set -euo pipefail

COMMAND="${*:-dotnet test src/Eventity.Tests.Integration/Eventity.Tests.Integration.csproj}"
OUT_DIR="${OUT_DIR:-artifacts/resource}"
OTEL_EXPORTER="${OTEL_EXPORTER:-Console}"
ROOT_DIR="$(pwd)"
ALLURE_DIR="$ROOT_DIR/artifacts/allure-results"

mkdir -p "$OUT_DIR"
mkdir -p "$ALLURE_DIR"

REPORT_PATH="$OUT_DIR/resource-report.md"
{
  echo "trace_off_default_logging -- трассировка выключена, логирование обычное"
  echo "trace_on_default_logging -- трассировка включена, логирование обычное"
  echo "trace_off_extended_logging -- трассировка выключена, логирование расширенное"
  echo "trace_on_extended_logging -- трассировка включена, логирование расширенное"
  echo ""
  echo "| Cценарий | U time (с) | S time (с) | Max RSS (Kб) |"
  echo "| --- | --- | --- | --- |"
} > "$REPORT_PATH"

run_case() {
  local name="$1"
  local trace_enabled="$2"
  local logging_mode="$3"

  local time_file="$OUT_DIR/${name}_time.txt"
  local otel_file="$OUT_DIR/${name}_otel.log"

  # choose a portable time command: prefer GNU 'gtime' on mac, otherwise use GNU time on linux
  if [ "$(uname)" = "Darwin" ]; then
    if command -v gtime >/dev/null 2>&1; then
      TIME_CMD="gtime -v"
      TIME_FORMAT="gnu"
    else
      TIME_CMD="/usr/bin/time -l"
      TIME_FORMAT="bsd"
    fi
  else
    TIME_CMD="/usr/bin/time -v"
    TIME_FORMAT="gnu"
  fi

  set +e
  (
    export ALLURE_RESULTS_DIRECTORY="$ALLURE_DIR"
    export OpenTelemetry__Enabled="$trace_enabled"
    export Logging__Mode="$logging_mode"
    export OpenTelemetry__Exporter="$OTEL_EXPORTER"
    export TEST_DB_HOST="${TEST_DB_HOST:-127.0.0.1}"
    # ensure per-project 'artifacts' folders exist so Allure can write into build output
    find "$ROOT_DIR/src" -type d -path "*/bin/*" -exec mkdir -p "{}/artifacts" \; || true

    # run command under chosen time tool; use non-login shell to avoid sourcing user profiles
    $TIME_CMD bash -c "cd \"$ROOT_DIR\" && $COMMAND"
  ) > "$otel_file" 2> "$time_file" || true
  local exit_code=$?
  set -e

  if [ "$exit_code" -ne 0 ]; then
    echo "Scenario $name failed with exit code $exit_code" >&2
    echo "See $otel_file and $time_file for details." >&2
    # don't abort the whole script: record failure in the report and continue
    printf "| %s | %s | %s | %s | # exit=%d\n" "$name" "n/a" "n/a" "n/a" "$exit_code" >> "$REPORT_PATH"
    return 0
  fi

  local user_time
  local system_time
  local max_rss
  if [ "${TIME_FORMAT:-gnu}" = "gnu" ]; then
    user_time=$(grep -m1 "User time" "$time_file" | awk -F: '{gsub(/^[ \t]+/, "", $2); print $2}')
    system_time=$(grep -m1 "System time" "$time_file" | awk -F: '{gsub(/^[ \t]+/, "", $2); print $2}')
    max_rss=$(grep -i -m1 "Maximum resident set size" "$time_file" | awk -F: '{gsub(/^[ \t]+/, "", $2); print $2}')
  else
    user_time=$(grep -i -m1 "user" "$time_file" | awk -F: '{gsub(/^[ \t]+/, "", $2); print $2}' || true)
    system_time=$(grep -i -m1 "system" "$time_file" | awk -F: '{gsub(/^[ \t]+/, "", $2); print $2}' || true)
    max_rss=$(grep -i -m1 "maximum resident set size" "$time_file" | awk '{print $1}' || true)
  fi

  printf "| %s | %s | %s | %s |\n" \
    "$name" \
    "${user_time:-n/a}" \
    "${system_time:-n/a}" \
    "${max_rss:-n/a}" >> "$REPORT_PATH"
}

run_case "trace_off_default_logging" "false" "Default"
run_case "trace_on_default_logging" "true" "Default"
run_case "trace_off_extended_logging" "false" "Extended"
run_case "trace_on_extended_logging" "true" "Extended"

echo "Resource report saved to: $REPORT_PATH"
echo "OpenTelemetry output saved to: $OUT_DIR/*_otel.log"
