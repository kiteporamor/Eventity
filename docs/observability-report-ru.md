# Отчет по интеграции трассировки/мониторинга и проверке требований

## Что сделано в проекте

1. Интегрирована OpenTelemetry трассировка и метрики:
   - Добавлены зависимости OpenTelemetry в `src/Directory.Build.props`.
   - Добавлен сервис-расширение `AddOpenTelemetryIfEnabled` в `src/Eventity.Web/OpenTelemetryExtensions.cs`.
   - Подключение трассировки и метрик включается настройкой `OpenTelemetry:Enabled`.
   - Экспортер выбирается через `OpenTelemetry:Exporter` (`Console` или `Otlp`).

2. Добавлены переключатели логирования:
   - В `src/Eventity.Web/Program.cs` добавлен режим логирования `Logging:Mode`.
   - `Default` = логирование по конфигу и обычный уровень.
   - `Extended` = Debug-уровень + request logging (`UseSerilogRequestLogging`).

3. Добавлены конфигурации:
   - `src/Eventity.Web/appsettings.json`
   - `src/Eventity.Web/appsettings.Development.json`
   - `src/Eventity.Tests.E2E/appsettings.json`
   - Содержат `OpenTelemetry` и `Logging` секции.

4. Добавлен скрипт замеров ресурсов:
   - `scripts/measure_resources.sh` запускает тесты/benchmark в 4 сценариях и формирует таблицу.
   - Сценарии:
     - `trace_off_default_logging`
     - `trace_on_default_logging`
     - `trace_off_extended_logging`
     - `trace_on_extended_logging`

5. Добавлен шаблон отчета:
   - `docs/monitoring-report.md` содержит таблицу для заполнения и инструкции запуска.

## Как проверить соответствие требованиям (по пунктам)

### 1. Данные мониторинга доступны при запуске тестов из CI/CD и benchmark-теста

Проверка:
1) Включить трассировку:
   - Установить `OpenTelemetry__Enabled=true`.
2) Запустить скрипт:
   - В CI (Linux):
     ```bash
     ./scripts/measure_resources.sh "dotnet test src/Eventity.Tests.Unit/Eventity.Tests.Unit.csproj"
     ```
   - Для benchmark заменить команду в кавычках.
3) Убедиться, что создано:
   - `artifacts/resource/resource-report.md`
   - `artifacts/resource/*_otel.log`

Ожидаемый результат: артефакты сохраняются и могут быть собраны CI/CD.

### 2. Сравнение ресурсов при трассировке и без нее

Проверка:
1) Открыть `artifacts/resource/resource-report.md`.
2) Сравнить строки:
   - `trace_off_*` против `trace_on_*`.
3) Сравнить колонки:
   - User time (s)
   - System time (s)
   - Max RSS (KB)

Ожидаемый результат: видна разница затрат по трассировке.

### 3. Сравнение ресурсов при логировании по умолчанию и расширенном

Проверка:
1) В той же таблице сравнить строки:
   - `*_default_logging` против `*_extended_logging`.
2) Убедиться, что `Logging__Mode=Extended` включает Debug и request logging.

Ожидаемый результат: видна разница затрат на расширенное логирование.

### 4. Отчет с таблицей сравнений

Проверка:
1) В отчете `artifacts/resource/resource-report.md` есть таблица:
   ```
   | Scenario | User time (s) | System time (s) | Max RSS (KB) |
   | --- | --- | --- | --- |
   | trace_off_default_logging | ... | ... | ... |
   | trace_on_default_logging | ... | ... | ... |
   | trace_off_extended_logging | ... | ... | ... |
   | trace_on_extended_logging | ... | ... | ... |
   ```
2) Таблица заполнена данными после запуска скрипта.

Ожидаемый результат: таблица соответствует формату задания.

## Быстрые команды для проверки

- Тесты (CI/Linux):
  ```bash
  ./scripts/measure_resources.sh "dotnet test src/Eventity.Tests.Unit/Eventity.Tests.Unit.csproj"
  ```

- Benchmark (пример):
  ```bash
  ./scripts/measure_resources.sh "docker-compose -f docker-compose.yml up --build --abort-on-container-exit"
  ```

## Примечания

- Скрипт `scripts/measure_resources.sh` рассчитан на Linux (использует `/usr/bin/time -v`).
- Для OTLP-экспорта задайте:
  - `OpenTelemetry__Exporter=Otlp`
  - `OpenTelemetry__OtlpEndpoint=http://localhost:4317`
