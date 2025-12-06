#!/bin/bash

# Запуск E2E тестов через docker-compose
echo "=== Запуск E2E тестов 2FA ==="

# 1. Собрать и запустить всё
docker-compose -f docker-compose.test.2fa.yaml down --volumes  # Очистить предыдущие запуски
docker-compose -f docker-compose.test.2fa.yaml build --no-cache  # Пересобрать образы
docker-compose -f docker-compose.test.2fa.yaml up --abort-on-container-exit --exit-code-from e2e-2fa-tests

# 2. Проверить результат
TEST_EXIT_CODE=$?

# 3. Очистка
docker-compose -f docker-compose.test.2fa.yaml down --volumes

# 4. Вывести результат
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "Тесты прошли успешно!"
else
    echo "Тесты завершились с ошибкой (код: $TEST_EXIT_CODE)"
    exit $TEST_EXIT_CODE
fi