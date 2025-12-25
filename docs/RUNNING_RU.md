# Пояснение по работе кода и запуску

## 1. Архитектура после декомпозиции

Бэкенд разделен на три сервиса и взаимодействует синхронно по REST:

1) Gateway (API Gateway)
   - Проект: `src/Eventity.Web`
   - Роль: единственная точка входа для клиента, контроллеры API.
   - Общается с Core Service по REST.
   - Порт: 5001

2) Core Service (бизнес-логика)
   - Проект: `src/Eventity.CoreService`
   - Роль: бизнес-логика, доменные проверки и правила.
   - Общается с Data Service по REST.
   - Порт: 5002

3) Data Service (доступ к данным)
   - Проект: `src/Eventity.DataService`
   - Роль: доступ к БД, репозитории и EF Core.
   - Работает с PostgreSQL.
   - Порт: 5003

Доменные модели (`Eventity.Domain`) используются как DTO между сервисами.

## 2. Основные маршруты взаимодействия

Gateway -> Core Service:
- `http://core-service:5002/core/v1/*`

Core Service -> Data Service:
- `http://data-service:5003/data/v1/*`

Как это работает:
- Все вызовы синхронные (REST HTTP).
- Gateway принимает запросы клиента, преобразует DTO и вызывает Core Service.
- Core Service выполняет бизнес-логику и обращается к Data Service за данными.
- Data Service работает с PostgreSQL и возвращает доменные модели обратно.

Health-check:
- Gateway: `GET /api/v1/health`
- Core: `GET /core/v1/health`
- Data: `GET /data/v1/health`

## 8. Последовательность вызовов при регистрации

1) Клиент вызывает Gateway:
   - `POST /api/v1/auth/register`
   - Контроллер: `src/Eventity.Web/Controllers/AuthController.cs`

2) Gateway вызывает Core Service:
   - Клиент `CoreAuthServiceClient` (`src/Eventity.Web/CoreClients/CoreAuthServiceClient.cs`)
   - HTTP: `POST http://core-service:5002/core/v1/auth/register`
   - Тело: `AuthRegisterRequest` (доменные DTO)

3) Core Service обрабатывает регистрацию:
   - Контроллер: `src/Eventity.CoreService/Controllers/AuthController.cs`
   - Сервис: `AuthService.RegisterUser(...)` (`src/Eventity.Application/Services/AuthService.cs`)

4) Внутри `AuthService.RegisterUser(...)`:
   - Проверка наличия пользователя:
     - Репозиторий: `IUserRepository.GetByLoginAsync`
     - Реализация: `DataServiceUserRepository` (`src/Eventity.CoreService/DataClients/DataServiceUserRepository.cs`)
     - HTTP: `GET http://data-service:5003/data/v1/users/by-login/{login}`
   - Если пользователь не найден, создается новый:
     - Репозиторий: `IUserRepository.AddAsync`
     - Реализация: `DataServiceUserRepository`
     - HTTP: `POST http://data-service:5003/data/v1/users`

5) Data Service сохраняет пользователя в PostgreSQL:
   - Контроллер: `src/Eventity.DataService/Controllers/UsersController.cs`
   - Репозиторий EF: `UserRepository` (`src/Eventity.DataAccess/Repositories/UserRepository.cs`)

6) Core Service формирует токен:
   - Сервис: `JwtService.GenerateToken(...)` (`src/Eventity.Application/Services/JwtService.cs`)
   - Возвращает `AuthResult` с токеном.

7) Ответ возвращается обратно:
   - Core -> Gateway -> Клиент

## 3. Логи (monitoring)

Все сервисы пишут логи в каталог `monitoring/`:
- `monitoring/gateway-service.log`
- `monitoring/core-service.log`
- `monitoring/data-service.log`

При запуске через Docker этот каталог пробрасывается как volume в контейнеры.

## 4. Запуск через Docker Compose (рекомендуется)

Требования:
- Docker Desktop

Команда запуска (из корня проекта):
```bash
docker-compose -f docker-compose.nginx.yml up --build
```

Если хотите воспользоваться готовым скриптом (Linux/macOS):
```bash
bash bash/nginx/run.sh
```

После запуска:
- API доступен через Nginx по адресу: `http://localhost/api/v1/...`
- Swagger доступен: `http://localhost/swagger`
- Adminer: `http://localhost/admin/`

## 5. Репликация и балансировка

В `docker-compose.nginx.yml` настроены три инстанса каждого сервиса:
- `gateway` x3
- `core-service` x3
- `data-service` x3

Nginx использует upstream и Docker DNS (`resolver 127.0.0.11`) для балансировки
по репликам `gateway`.

## 6. Запуск без Docker (локально)

Последовательность:
1) Поднять PostgreSQL на `localhost:5432`
2) Запустить Data Service:
   - `dotnet run --project src/Eventity.DataService/Eventity.DataService.csproj`
3) Запустить Core Service:
   - `dotnet run --project src/Eventity.CoreService/Eventity.CoreService.csproj`
4) Запустить Gateway:
   - `dotnet run --project src/Eventity.Web/Eventity.Web.csproj`

Проверьте, что в `appsettings.Development.json` для Gateway
указан `ServiceUrls:CoreService = http://localhost:5002`.

## 7. Ключевые конфигурации

Gateway (`src/Eventity.Web/appsettings*.json`):
- `ServiceUrls:CoreService`
- `Jwt:*`

Core Service (`src/Eventity.CoreService/appsettings.json`):
- `ServiceUrls:DataService`
- `Jwt:*`

Data Service (`src/Eventity.DataService/appsettings.json`):
- `ConnectionStrings:DataBaseConnect`

## 8. Новая схема балансировки (GET vs WRITE)

Nginx распределяет GET-запросы к `/api/v1` и `/api/v2` между тремя Gateway:
- `gateway-main` (вес 2)
- `gateway-ro1` (вес 1)
- `gateway-ro2` (вес 1)

Все POST/PUT/PATCH/DELETE идут только на `gateway-main`.
Read-only инстансы отдают `403` при попытке записи.

## 9. Зеркальная версия (/mirror)

Для `/mirror` подняты отдельные инстансы Gateway:
- `gateway-mirror-main`, `gateway-mirror-ro1`, `gateway-mirror-ro2`

Пути:
- `/mirror`
- `/mirror/api/v1`
- `/mirror/swagger`

## 10. Репликация PostgreSQL

В `docker-compose.nginx.yml` подняты две БД:
- `postgres-primary` — мастер (запись)
- `postgres-replica` — реплика (чтение)

Read-only Data Service использует реплику и `default_transaction_read_only=on`.

## 11. Мониторинг логов (/monitoring)

Развернута связка Grafana + Loki + Promtail.
Grafana доступна по адресу: `http://localhost/monitoring/`
Логи собираются из `monitoring/*.log` со всех инстансов.
