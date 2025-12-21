#!/bin/bash

echo "=== БАЛАНСИРОВКА ==="

echo "Статусы контейнеров:"

echo "Создание read-only пользователя:"
docker-compose -f docker-compose.nginx.yml exec postgres psql -U postgres -d Eventity -c "
SELECT usename, usecreatedb, usesuper 
FROM pg_user 
WHERE usename IN ('postgres', 'postgres_readonly');
" 2>/dev/null || echo "⚠️  Проверьте создание пользователя после полного запуска"
echo ""

echo "Доступность бэкендов:"
echo "----------------------------------"
echo "Основной (порт 5001):"
if curl -s http://localhost:5001/api/v1/health > /dev/null; then
    echo "Работает"
else
    echo "Не работает"
fi

echo "Read-only #1 (порт 5002):"
if curl -s http://localhost:5002/api/v1/health > /dev/null; then
    echo "Работает"
else
    echo "Не работает"
fi

echo "Read-only #2 (порт 5003):"
if curl -s http://localhost:5003/api/v1/health > /dev/null; then
    echo "Работает"
else
    echo "Не работает"
fi
echo ""

echo "Балансировка GET запросов (10 запросов):"
echo "-----------------------------------------------------"
echo "Запросы через nginx (порт 80):"
for i in {1..10}; do
    echo -n "Запрос $i: "
    curl -s http://localhost/api/v1/health
    echo ""
    sleep 0.5
done
echo ""

echo "Маршрутизация POST запросов:"
echo "-----------------------------------------"
echo "POST запрос без авторизации (должен быть 401):"
curl -s -X POST http://localhost/api/v1/events \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","description":"Test"}' \
  -w " Статус: %{http_code}\n" \
  -o /dev/null
echo ""

echo "read-only ограничения:"
echo "------------------------------------"
echo "Прямой POST к read-only бэкенду #1 (должен быть 403):"
curl -s -X POST http://localhost:5002/api/v1/events \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","description":"Test"}' \
  -w " Статус: %{http_code}\n" \
  -o /dev/null

echo "POST к read-only бэкенду #2 (должен быть 403):"
curl -s -X POST http://localhost:5003/api/v1/events \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","description":"Test"}' \
  -w " Статус: %{http_code}\n" \
  -o /dev/null
echo ""

