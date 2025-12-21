const backends = [
    { id: 'write', port: 5001, mode: 'write', name: 'Основной бэкенд' },
    { id: 'read-1', port: 5002, mode: 'read', name: 'Бэкенд только для чтения #1' },
    { id: 'read-2', port: 5003, mode: 'read', name: 'Бэкенд только для чтения #2' }
];

let stats = {
    totalGet: 0,
    totalWrite: 0,
    writeErrors: 0,
    distribution: [0, 0, 0],
    backendRequests: [0, 0, 0]
};

async function checkBackendHealth(backend) {
    try {
        const response = await fetch(`http://localhost:${backend.port}/api/v1/health`, {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        });
        
        return response.ok;
    } catch (error) {
        return false;
    }
}

async function updateBackendStatus() {
    for (const [index, backend] of backends.entries()) {
        const isHealthy = await checkBackendHealth(backend);
        const statusElement = document.getElementById(`status-${backend.id}`);
        
        if (isHealthy) {
            statusElement.textContent = 'Работает';
            statusElement.className = 'status healthy';
        } else {
            statusElement.textContent = 'Недоступен';
            statusElement.className = 'status unhealthy';
        }
        
        // Обновляем статистику запросов (имитация)
        if (isHealthy) {
            stats.backendRequests[index] += Math.floor(Math.random() * 10) + 1;
            document.getElementById(`requests-${backend.id}`).textContent = stats.backendRequests[index];
        }
    }
    
    // Обновляем общую статистику
    stats.totalGet = stats.backendRequests.reduce((a, b) => a + b, 0);
    stats.totalWrite = Math.floor(stats.totalGet * 0.3); // Пример: 30% запросов на запись
    stats.writeErrors = Math.floor(Math.random() * 5); // Пример случайных ошибок
    
    document.getElementById('total-get').textContent = stats.totalGet;
    document.getElementById('total-write').textContent = stats.totalWrite;
    document.getElementById('write-errors').textContent = stats.writeErrors;
    document.getElementById('distribution').textContent = stats.backendRequests.join(':');
    
    // Обновляем время
    document.getElementById('last-update').textContent = new Date().toLocaleTimeString();
}

// Функция для тестирования балансировки
async function testLoadBalancing() {
    const testResults = {
        write: { success: 0, error: 0 },
        read: { success: 0, error: 0 }
    };
    
    // Тест GET запросов (должны распределяться)
    console.log('Testing GET requests distribution...');
    for (let i = 0; i < 10; i++) {
        try {
            const response = await fetch('/api/v1/health');
            testResults.read.success++;
        } catch (error) {
            testResults.read.error++;
        }
    }
    
    // Тест POST запросов (должны идти только на write)
    console.log('Testing POST requests (should go to write backend)...');
    try {
        const response = await fetch('/api/v1/events', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ test: true })
        });
        
        if (response.status === 403) {
            testResults.write.success++; // Ожидаем ошибку для readonly бэкендов
        } else if (response.ok) {
            testResults.write.success++;
        } else {
            testResults.write.error++;
        }
    } catch (error) {
        testResults.write.error++;
    }
    
    console.log('Test results:', testResults);
    return testResults;
}

// Обновляем статус при загрузке страницы и каждые 5 секунд
updateBackendStatus();
setInterval(updateBackendStatus, 5000);

// Экспортируем функцию для кнопки
window.updateLoadBalancerStatus = updateBackendStatus;

// Запускаем тест при загрузке (опционально)
window.addEventListener('load', () => {
    setTimeout(testLoadBalancing, 2000);
});