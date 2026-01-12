#!/bin/bash
# run_degradation_get_test.sh

echo "--- Тест деградации с GET запросами ---"

RESULTS_DIR="degradation_get_test_$(date +%Y%m%d_%H%M%S)"
mkdir -p $RESULTS_DIR

echo "Результаты будут сохранены в: ${RESULTS_DIR}"

echo "Запуск Docker окружения..."
docker-compose -f docker-compose.dev.yml down 
docker-compose -f docker-compose.dev.yml up -d --build

echo "Ожидание готовности сервисов и загрузки данных..."
for i in {1..30}; do
  if curl -sf http://localhost:7080/health > /dev/null 2>&1; then
    echo "API доступно (попытка $i)"
    sleep 5
    break
  fi
  echo "Ожидание API... (попытка $i/30)"
  sleep 2
done

echo "Финальная проверка доступности API..."
if curl -f http://localhost:7080/health; then
    echo " API готово"
else
    echo "API недоступно"
    docker-compose -f docker-compose.dev.yml logs app
    exit 1
fi

echo "Запуск мониторинга ресурсов..."
STATS_FILE="${RESULTS_DIR}/resource_usage.csv"
echo "timestamp,container,cpu_percent,mem_usage,mem_percent,net_io,block_io,pids" > $STATS_FILE


# подготовить идентификаторы контейнеров для мониторинга
APP_CID=$(docker-compose -f docker-compose.dev.yml ps -q app)
DB_CID=$(docker-compose -f docker-compose.dev.yml ps -q db)
# запуск фонового процесса мониторинга
(while true; do
    TIMESTAMP=$(date +%s%3N)
    docker stats --no-stream --format \
        "$TIMESTAMP,{{.Name}},{{.CPUPerc}},{{.MemUsage}},{{.MemPerc}},{{.NetIO}},{{.BlockIO}},{{.PIDs}}" \
        $APP_CID $DB_CID >> $STATS_FILE 2>/dev/null || true
    sleep 2
done) &

#$! содержит PID последнего фонового процесса,
STATS_PID=$!

echo "Запуск теста деградации с GET запросами..."

# Создание контейнер (не запуск)
CONTAINER_ID=$(docker create \
    --network src_app-network \
    grafana/k6:0.49.0 \
    run /test.js \
    --out json=/tmp/k6_results.json \
    --out csv=/tmp/k6_results.csv)

docker cp scripts/k6/degradation_test_get.js $CONTAINER_ID:/test.js

echo "Запуск теста..."
docker start -a $CONTAINER_ID
K6_EXIT_CODE=$?

echo "Копирование результатов..."
if [ -n "$CONTAINER_ID" ]; then
    docker cp $CONTAINER_ID:/tmp/k6_results.json ${RESULTS_DIR}/ && echo "✅ JSON результаты скопированы"
    docker cp $CONTAINER_ID:/tmp/k6_results.csv ${RESULTS_DIR}/ && echo "✅ CSV результаты скопированы"
    docker rm $CONTAINER_ID > /dev/null 2>&1 || true
fi

echo "Остановка мониторинга..."
kill $STATS_PID 2>/dev/null
wait $STATS_PID 2>/dev/null

if [ -f "${RESULTS_DIR}/k6_results.json" ] && [ $K6_EXIT_CODE -eq 0 ]; then
    echo ">>>>>>>>>>>>>> Тест выполнен успешно"
    echo "Анализ результатов..."
    if [ -f "analyze_copy.py" ]; then
        python3 analyze_copy.py "${RESULTS_DIR}"
    else
        echo ">>>>>>>>>>>>>>>>  Скрипт анализа не найден"
    fi
else
    echo ">>>>>>>>>>>>>>>>>>> Тест завершился с ошибкой (код: $K6_EXIT_CODE)"
    
    echo "--- Последние логи приложения ---"
    docker compose logs app --tail=20
fi

echo "Остановка Docker окружения..."
docker-compose -f docker-compose.dev.yml down 

echo "--- Тест деградации с GET запросами завершен ---"
if [ -f "${RESULTS_DIR}/k6_results.json" ]; then
    echo ">>>>>>>>>>> Результаты в: ${RESULTS_DIR}"
    ls -la ${RESULTS_DIR}/
else
    echo ">>>>>>>>>>> Тест не выполнен."
fi