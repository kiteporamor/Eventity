import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.API_URL || 'http://localhost:5001';
let authToken = '';

export function setup() {
    const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
        login: 'admin',
        password: 'adminpass'
    }), {
        headers: { 'Content-Type': 'application/json' },
    });
    
    if (loginRes.status !== 200) {
        console.error(`Admin login failed: ${loginRes.status}`);
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

    // Запрос с параметрами (если есть данные)
    const participationsRes = http.get(
        `${BASE_URL}/api/participations`, 
        { headers }
    );
    
    check(participationsRes, {
        'participations query successful': (r) => r.status === 200 || r.status === 204,
        'response time acceptable': (r) => r.timings.duration < 1000,
    });

    // Запрос пользователей
    const usersRes = http.get(`${BASE_URL}/api/users`, { headers });
    
    check(usersRes, {
        'user search successful': (r) => r.status === 200,
    });

    sleep(1);
}