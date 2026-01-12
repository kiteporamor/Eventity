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
    high_load: {
      executor: 'constant-arrival-rate',
      rate: 600,
      timeUnit: '1s',
      duration: '6m', 
      preAllocatedVUs: 500, 
    },
  },
  discardResponseBodies: false,
};

export default function () {
  const geteventsRes = http.get(`${BASE_URL}/events`);
  requestCount.add(1);
  
  const isSuccess = check(geteventsRes, { 
    'GET /events status is 200': (r) => r.status === 200,
  });
  
  if (!isSuccess) {
    errorCount.add(1);
    console.log(`GET request failed with status: ${geteventsRes.status}, body: ${geteventsRes.body}`);
  }
  successRate.add(isSuccess);
  
  responseTime.add(geteventsRes.timings.duration);

  return
}