import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.API_URL || 'http://localhost:5001';

export default function() {
    // Тест логина с существующим пользователем
    const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
        login: 'testuser',
        password: 'testpass'
    }), {
        headers: { 'Content-Type': 'application/json' },
    });

    check(loginRes, {
        'login successful': (r) => r.status === 200,
        'login response time < 300ms': (r) => r.timings.duration < 300,
    });
}