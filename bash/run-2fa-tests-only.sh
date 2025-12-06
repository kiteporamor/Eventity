#!/bin/bash

docker-compose -f docker-compose.test.2fa.yaml down --volumes 
docker-compose -f docker-compose.test.2fa.yaml build --no-cache 
docker-compose -f docker-compose.test.2fa.yaml up --abort-on-container-exit --exit-code-from e2e-2fa-tests

TEST_EXIT_CODE=$?

docker-compose -f docker-compose.test.2fa.yaml down --volumes

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "Тесты прошли успешно!"
else
    echo "Тесты завершились с ошибкой (код: $TEST_EXIT_CODE)"
    exit $TEST_EXIT_CODE
fi