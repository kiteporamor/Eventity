# Eventity — краткое руководство (RU)

Это краткая документация по тому, что было добавлено, как это работает в целом и как запускать в CI/CD на GitHub.

## Что добавлено

- Проектная интеграция мониторинга производительности и базовой телеметрии.
- Три профиля конфигурации приложения:
  - `appsettings.minimal.json` — базовый прод (минимальный overhead).
  - `appsettings.extended.json` — разработка (детальные логи, SQL).
  - `appsettings.telemetry.json` — наблюдаемость (расширенные логи, телеметрия).
- Новый проект для тестов производительности `src/Eventity.Benchmarks/` с:
  - `TelemetryBenchmark.cs` — сценарии нагрузочных тестов.
  - `ResourceMeasurement.cs` — замер CPU/памяти/потоков и генерация отчётов.
  - `TelemetryIntegrationTests.cs` — интеграционные тесты, собирающие метрики.
- Инфраструктура:
  - `docker-compose.benchmarks.yml` — запуск БД и бенчмарков в контейнерах.
  - `Dockerfile.benchmarks` — образ для изолированного прогона.
  - `.github/workflows/benchmarks.yml` — CI для автоматического запуска тестов/бенчмарков и выгрузки отчётов.

## Как это работает (вкратце)

- Веб-приложение читает один из профилей `appsettings.*.json`, определяя уровень логирования и включение телеметрии.
- Бенчмарки и интеграционные тесты запускают типовые сценарии (регистрация, аутентификация, создание события, комплексный сценарий) и меряют:
  - Время выполнения (Elapsed / Processor time)
  - Память (дельта выделений)
  - Количество потоков и % использования CPU
- Результаты сохраняются в `reports/` и `logs/` (создаются во время прогона) и могут собираться как артефакты CI.
>   1
## Как запустить локально

1) Восстановить и собрать решение:
```bash
cd /Users/ekaterinaparamonova/Downloads/Eventity
dotnet restore src/Eventity.sln
dotnet build src/Eventity.sln -c Release
```

2) Поднять PostgreSQL (одно из):
```bash
# Docker
docker run -d \
  -p 5432:5432 \
  -e POSTGRES_DB=eventity_benchmark \
  -e POSTGRES_PASSWORD=postgres \
  postgres:15-alpine

# или локальная БД (пример)
createdb eventity_benchmark || true
```

3) Запустить быстрый прогон бенчмарков/интеграционных тестов:
```bash
cd src/Eventity.Benchmarks
# быстрый прогон
dotnet test
# полный бенчмарк-сет
dotnet run -c Release -- --runtimes net60
```

4) Посмотреть результаты (если сгенерированы):
```bash
# отчёты и логи появляются при прогонах
ds reports || true
ls -l reports || true
ls -l logs || true
```

## Как запустить в GitHub Actions (CI/CD)

В репозитории уже есть workflow: `.github/workflows/benchmarks.yml`.
Он делает следующее:
- Собирает решение и запускает тесты/бенчмарки для трёх профилей (Minimal/Extended/Telemetry) параллельно.
- Сохраняет отчёты из `reports/` как артефакты сборки.
- Может выполняться по push/pr и по расписанию.

Если нужно включить/настроить расписание — проверьте секцию `on.schedule` в `.github/workflows/benchmarks.yml`.

Ручной запуск:
- В GitHub → Actions → выберите «Performance Benchmarking & Monitoring» → «Run workflow».

## Полезные команды

```bash
# Очистить NuGet-кэш при ошибках зависимостей
dotnet nuget locals all --clear

# Переустановить и пересобрать
dotnet restore src/Eventity.sln
dotnet build src/Eventity.sln -c Release

# Проверить, что PostgreSQL доступен
psql -U postgres -c "SELECT 1" || true
```

## Где искать конфигурации

- Профили приложения: `src/Eventity.Web/appsettings.minimal.json`, `src/Eventity.Web/appsettings.extended.json`, `src/Eventity.Web/appsettings.telemetry.json`.
- Бенчмарки и метрики: `src/Eventity.Benchmarks/`.
- CI-конвейер: `.github/workflows/benchmarks.yml`.
- Docker окружение: `docker-compose.benchmarks.yml`, `Dockerfile.benchmarks`.

---

Коротко: добавлена наблюдаемость (логи/телеметрия), бенчмарки, отчёты и CI-прогон. Для локального старта — `dotnet test` в `src/Eventity.Benchmarks`; для CI — уже есть workflow в `.github/workflows/benchmarks.yml`.
