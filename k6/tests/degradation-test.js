import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate, Gauge, Trend } from 'k6/metrics';

const responseTimes = new Trend('http_req_duration', true);
const degradationDetected = new Counter('degradation_detected');
const errorRate = new Rate('error_rate');
const throughput = new Counter('throughput');
const activeUsers = new Gauge('active_users');

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
        console.error(`Login failed: ${loginRes.status}`);
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

    activeUsers.add(1);

    // Тест получения событий (только чтение)
    const getRes = http.get(`${BASE_URL}/api/events`, { headers });
    
    responseTimes.add(getRes.timings.duration);
    throughput.add(1);
    
    const success = check(getRes, {
        'events retrieved': (r) => r.status === 200 || r.status === 204,
    });

    if (!success) {
        errorRate.add(1);
        if (getRes.timings.duration > 2000) {
            degradationDetected.add(1);
        }
    }

    activeUsers.add(-1);
    sleep(0.1);
}