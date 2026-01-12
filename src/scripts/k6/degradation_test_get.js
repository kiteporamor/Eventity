import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';

const responseTime = new Trend('get_events_response_time');
const requestCount = new Counter('total_get_requests');
const errorCount = new Counter('total_get_errors');
const successRate = new Rate('get_success_rate');

const BASE_URL = 'http://app:5001/api';

export const options = {
    stages: [
    { duration: '1m', target: 1 },
    { duration: '3m', target: 10 },
    { duration: '3m', target: 100 },
    { duration: '3m', target: 500 },
    { duration: '3m', target: 1000 },
  ],
};

export default function () {

  const getEventsRes = http.get(`${BASE_URL}/events`);
  requestCount.add(1);

  const isSuccess = check(getEventsRes, {
    'GET /events status is 200': (r) => r.status === 200,
  });

  if (!isSuccess) {
    errorCount.add(1);
    console.log(`GET /events failed with status: ${getEventsRes.status}, body: ${getEventsRes.body}`);
  }
  successRate.add(isSuccess);

  responseTime.add(getEventsRes.timings.duration);

  sleep(1);
}


