#!/bin/bash

echo "=== Eventity Benchmark: k6 → InfluxDB → Grafana ==="

# Очистка
docker-compose -f docker-compose.benchmark.yml down --remove-orphans

# Запуск
docker-compose -f docker-compose.benchmark.yml up -d --build

echo "Waiting for services..."
sleep 30

# Проверка готовности
echo "Checking services..."
until curl -s http://localhost:5002/health > /dev/null; do
    echo "Waiting for PostgreSQL app..."
    sleep 5
done

until curl -s http://localhost:5003/health > /dev/null; do
    echo "Waiting for MongoDB app..."
    sleep 5 
done

docker-compose -f docker-compose.benchmark.yml up -d data-generator

run_test() {
    local test_name=$1
    local db_type=$2
    local app_url=$3
    
    echo "=== Running $test_name - $db_type ==="
    
    docker-compose -f docker-compose.benchmark.yml run --rm k6-runner run \
        /scripts/tests/$test_name \
        -e API_URL=$app_url \
        --out influxdb=http://influxdb:8086/k6 \
        --tag "test_type=$test_name,db_type=$db_type"
}

run_test "simple-test.js" "postgresql" "http://app-postgres:5001"
run_test "simple-test.js" "mongodb" "http://app-mongo:5001"

# Тесты для PostgreSQL
run_test "serialization.test.js" "postgresql" "http://app-postgres:5001"
run_test "heavy-requests.test.js" "postgresql" "http://app-postgres:5001"
run_test "auth-load.test.js" "postgresql" "http://app-postgres:5001"
run_test "degradation-test.js" "postgresql" "http://app-postgres:5001"

# Тесты для MongoDB
run_test "serialization.test.js" "mongodb" "http://app-mongo:5001"
run_test "heavy-requests.test.js" "mongodb" "http://app-mongo:5001"
run_test "auth-load.test.js" "mongodb" "http://app-mongo:5001"
run_test "degradation-test.js" "mongodb" "http://app-mongo:5001"

echo "=== Benchmark Complete ==="
echo "InfluxDB: http://localhost:8086"
echo "Grafana: http://localhost:3000 (admin/admin)"
echo "Add InfluxDB datasource in Grafana: http://influxdb:8086, database: k6"