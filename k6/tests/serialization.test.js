import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.API_URL || 'http://localhost:5001';
let authToken = '';

export function setup() {
    const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
        login: 'testuser',
        password: 'testpass'
    }), {
        headers: { 'Content-Type': 'application/json' },
    });
    
    if (loginRes.status !== 200) {
        console.error(`Login failed: ${loginRes.status} - ${loginRes.body}`);
        return { token: null };
    }
    
    authToken = loginRes.json('token');
    return { token: authToken };
}

export default function(data) {
    if (!data.token) {
        console.log('No auth token, skipping iteration');
        return;
    }
    
    const headers = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${data.token}`
    };

    // Тест получения событий (только чтение)
    const getRes = http.get(`${BASE_URL}/api/events`, { headers });
    
    check(getRes, {
        'events retrieved': (r) => r.status === 200 || r.status === 204,
    });

    sleep(1);
}