import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.API_URL || 'http://localhost:5001';

export default function() {
    // Проверка health endpoint
    const healthRes = http.get(`${BASE_URL}/health`);
    
    check(healthRes, {
        'health status is 200': (r) => r.status === 200,
    });
    
    const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
        login: 'testuser',
        password: 'testpass'
    }), {
        headers: { 'Content-Type': 'application/json' },
    });
    
    check(loginRes, {
        'login status is 200': (r) => r.status === 200,
        'has token': (r) => r.json('token') !== undefined,
    });
    
    sleep(1);
}