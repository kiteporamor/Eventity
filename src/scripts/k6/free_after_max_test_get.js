import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';
const responseTime = new Trend('get_events_response_time');
const requestCount = new Counter('total_get_requests');
const errorCount = new Counter('total_get_errors');
const successRate = new Rate('get_success_rate');

const BASE_URL = 'http://app:5001/api';

export const options = {
  scenarios: {
    // выосокая нагрузка
    high_load_phase: {
      executor: 'constant-arrival-rate',
      rate: 1320, //2500 
      timeUnit: '1s',
      duration: '3m',
      preAllocatedVUs: 1060, 
    },
    // нормальная нагрузка
    normal_load_phase: {
      executor: 'constant-arrival-rate',
      rate: 300,
      timeUnit: '1s',
      duration: '3m',
      preAllocatedVUs: 400, 
      startTime: '3m', 
    },
  },
  discardResponseBodies: false,
};

export default function () { 
  const getEventsRes = http.get(`${BASE_URL}/events`);
  requestCount.add(1);
  
  const isSuccess = check(getEventsRes, { 
    'GET /events status is 200': (r) => r.status === 200,
  });
  
  if (!isSuccess) {
    errorCount.add(1);
    console.log(`GET request failed with status: ${getEventsRes.status}, body: ${getEventsRes.body}`);
  }
  successRate.add(isSuccess);
  
  responseTime.add(getEventsRes.timings.duration);

  return
}