export const options = {
    scenarios: {
        serialization_test: {
            executor: 'per-vu-iterations',
            vus: 10,
            iterations: 50,  // 500 запросов всего
            maxDuration: '5m',
        },
        heavy_requests_test: {
            executor: 'per-vu-iterations', 
            vus: 5,
            iterations: 20,  // 100 запросов
            maxDuration: '5m',
        },
        auth_load_test: {
            executor: 'per-vu-iterations',
            vus: 20, 
            iterations: 10,  // 200 запросов
            maxDuration: '5m',
        },
        degradation_test: {
            executor: 'ramping-arrival-rate',
            startRate: 10,
            timeUnit: '1s',
            preAllocatedVUs: 10,
            maxVUs: 100,
            stages: [
                { target: 20, duration: '1m' },
                { target: 40, duration: '1m' },
                { target: 60, duration: '1m' },
            ],
        }
    },
    thresholds: {
        http_req_duration: [
            'p(50)<500',
            'p(75)<800',  
            'p(90)<1200',
            'p(95)<1500',
            'p(99)<2000'
        ],
        http_req_failed: ['rate<0.1'],
    },
};