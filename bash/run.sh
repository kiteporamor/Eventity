#!/bin/bash

# 1. Проверить, что контейнер запущен
docker ps | grep eventity-app

# 2. Пробросить порт контейнера на localhost
docker port eventity-app 5001

# 3. ИЛИ пробросить при запуске контейнера
docker run -d -p 5001:5001 --name eventity-app ваш-образ

# 4. Затем запустить тесты с localhost
cd ./src/Eventity.Tests.E2E.FA
EVENTITY_API_URL=http://localhost:5001 dotnet test