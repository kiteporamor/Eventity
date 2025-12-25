# Запуск приложения (актуальная схема)

## Требования
- Docker Desktop
- Docker Compose

## 1. Запуск всех сервисов

Из корня проекта:
```bash
docker-compose -f docker-compose.nginx.yml up --build
```

После старта доступны:
- Основное приложение: `http://localhost/`
- API: `http://localhost/api/v1/`
- Swagger: `http://localhost/swagger`
- Зеркало: `http://localhost/mirror/`
- Зеркальный API: `http://localhost/mirror/api/v1/`
- Grafana (логи): `http://localhost/monitoring/` (admin/admin)
- Adminer: `http://localhost/admin/`

## 2. Проверка состояния сервисов

```bash
docker-compose -f docker-compose.nginx.yml ps
```

Все `gateway/core/data` (включая ro и mirror) должны быть `healthy`.

## 3. Быстрые проверки API

```bash
curl.exe -i http://localhost/api/v1/health
curl.exe -i http://localhost/mirror/api/v1/health
```

Ожидаемый результат: `200 OK` и `Healthy`.

## 4. Проверка read-only инстанса

```bash
docker-compose -f docker-compose.nginx.yml exec -T gateway-ro1 curl -i -X POST http://localhost:5004/api/v1/health
```

Ожидаемый результат: `403 Forbidden`.

## 5. Остановка

```bash
docker-compose -f docker-compose.nginx.yml down
```
