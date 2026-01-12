#!/bin/bash
# run_continious_max_test.sh


RESULTS_DIR="continious_max_test_$(date +%Y%m%d_%H%M%S)"
mkdir -p $RESULTS_DIR

echo "Результаты будут сохранены в: ${RESULTS_DIR}"

echo "Запуск Docker окружения..."
docker-compose -f docker-compose.dev.yml down 
docker-compose -f docker-compose.dev.yml up -d --build

echo "Ожидание готовности сервисов... (30 секунд)"
sleep 30

if curl -f http://localhost:7080/health > /dev/null 2>&1; then
    echo " API доступно"
else
    echo "API недоступно"
    docker compose logs app
    exit 1
fi

STATS_FILE="${RESULTS_DIR}/resource_usage.csv"
echo "timestamp,container,cpu_percent,mem_usage,mem_percent,net_io,block_io,pids" > $STATS_FILE

APP_CID=$(docker-compose -f docker-compose.dev.yml ps -q app)
DB_CID=$(docker-compose -f docker-compose.dev.yml ps -q db)

(while true; do
  TIMESTAMP=$(date +%s%3N)
  docker stats --no-stream --format \
    "$TIMESTAMP,{{.Name}},{{.CPUPerc}},{{.MemUsage}},{{.MemPerc}},{{.NetIO}},{{.BlockIO}},{{.PIDs}}" \
    $APP_CID $DB_CID >> $STATS_FILE 2>/dev/null || true
  sleep 2
done) &
STATS_PID=$!


CONTAINER_ID=$(docker create \
  --network src_app-network \
  grafana/k6:0.49.0 \
  run /test.js \
  --out json=/tmp/k6_results.json \
  --out csv=/tmp/k6_results.csv)

docker cp scripts/k6/continious_max_test_get.js $CONTAINER_ID:/test.js

docker start -a $CONTAINER_ID
K6_EXIT_CODE=$?

echo "Копирование результатов..."
if [ -n "$CONTAINER_ID" ]; then
  docker cp $CONTAINER_ID:/tmp/k6_results.json ${RESULTS_DIR}/ && echo "✅ JSON результаты скопированы"
  docker cp $CONTAINER_ID:/tmp/k6_results.csv ${RESULTS_DIR}/ && echo "✅ CSV результаты скопированы"
  docker rm $CONTAINER_ID > /dev/null 2>&1 || true
fi

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

    if [ -f "analyze_degradation.py" ]; then
        python3 analyze_degradation.py "${RESULTS_DIR}"
    else
        echo ">>>>>>>>>>>>>>>>  Скрипт анализа не найден"
    fi
else
    echo ">>>>>>>>>>>>>>>>>>> Тест завершился с ошибкой (код: $K6_EXIT_CODE)"
    
    echo "--- Последние логи приложения ---"
    docker compose logs app --tail=20
fi

docker-compose -f docker-compose.dev.yml down 

if [ -f "${RESULTS_DIR}/k6_results.json" ]; then
    echo ">>>>>>>>>>> Результаты в: ${RESULTS_DIR}"
    ls -la ${RESULTS_DIR}/
else
    echo ">>>>>>>>>>> Тест не выполнен."
fi