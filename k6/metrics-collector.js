import { Counter, Trend, Rate, Gauge } from 'k6/metrics';

export const customMetrics = {
    serializationDuration: new Trend('serialization_duration'),
    dbQueryDuration: new Trend('db_query_duration'),
    authSuccessRate: new Rate('auth_success_rate'),
    errorCount: new Counter('error_count'),
    memoryUsage: new Gauge('memory_usage'),
    cpuUsage: new Gauge('cpu_usage'),
};

export function collectMetrics(res, metricType, additionalTags = {}) {
    const tags = {
        endpoint: res.url,
        method: res.request.method,
        status: res.status,
        ...additionalTags
    };

    if (metricType === 'serialization') {
        customMetrics.serializationDuration.add(res.timings.duration, tags);
    }
    
    if (metricType === 'db_query') {
        customMetrics.dbQueryDuration.add(res.timings.duration, tags);
    }
    
    if (res.status >= 400) {
        customMetrics.errorCount.add(1, tags);
    }
}