# Load Testing Report (WebLab#5)

This report documents how to validate the weighted load‑balancing (2:1:1), read/write split, mirror routing, caching/gzip, and monitoring setup. All commands assume the project root and Docker Desktop running.

## 1) Environment
- Start stack: `docker-compose -f docker-compose.nginx.yml up -d --build`
- Health/weights: `curl http://localhost/loadbalancer-status`
- Services: `nginx` (LB), `eventity-app` (write), `eventity-app-readonly-1`/`-2` (read), `eventity-app-mirror` (mirror, read-only, DB replica), `postgres` + `postgres-replica`, `grafana/loki/promtail`.

## 2) GET load (balance 2:1:1)
### Command (host has ab)
```sh
ab -n 400 -c 40 http://localhost/api/v1/health
```

### Command (dockerized ab on project network)
```sh
docker run --rm --network eventity_app-network jordi/ab:2.3 \
  -n 400 -c 40 http://nginx/api/v1/health
```

### Verify distribution
```sh
docker-compose -f docker-compose.nginx.yml exec nginx \
  sh -c "grep '/api/v1/health' /var/log/nginx/access.log | awk -F'backend: ' '{print $2}' | sort | uniq -c"
```

Expected counts ≈ 200 for `eventity-app:5001`, ≈100 for each `eventity-app-readonly-1:5002` and `eventity-app-readonly-2:5003` (ratio 2:1:1).

## 3) Write load (only main backend)
### Command
```sh
echo '{"title":"LoadTest","description":"lb"}' > /tmp/payload.json
ab -n 50 -c 10 -p /tmp/payload.json -T application/json http://localhost/api/v1/events
```

### Verify routing
```sh
docker-compose -f docker-compose.nginx.yml exec nginx \
  sh -c "grep 'POST /api/v1/events' /var/log/nginx/access.log | awk -F'backend: ' '{print $2}' | sort | uniq -c"
```

Expected: 100% to `eventity-app:5001`. Hitting read-only directly should return 403 by middleware:
```sh
curl -i -X POST http://localhost:5002/api/v1/events -d '{}' -H "Content-Type: application/json"
```

## 4) Mirror path (/mirror)
- Health: `curl http://localhost/mirror/api/v1/health`
- Write protection (should be 403): `curl -i -X POST http://localhost/mirror/api/v1/events -d '{}' -H "Content-Type: application/json"`
- Load sample: `ab -n 100 -c 20 http://localhost/mirror/api/v1/health`
- Verify: `grep '/mirror/api/v1/health' /var/log/nginx/access.log | awk -F'backend: ' '{print $2}' | sort | uniq -c` (all to `eventity-app-mirror:5004`).

## 5) Caching and gzip
- Static caching: `curl -I http://localhost/static/css/app.css` (expect `Cache-Control: public, immutable` and `Content-Encoding: gzip`/`.gz` hits).
- API not cached: `curl -I http://localhost/api/v1/health` (no cache headers, Server header overridden to app name).

## 6) Monitoring (Grafana + Loki)
- UI: `http://localhost/monitoring/` (Grafana root configured with sub-path).
- Logs: dashboard UID `eventity-monitoring` shows streams from all app instances + nginx.
- Loki API test: `curl "http://localhost/loki/api/v1/labels"`.

## 7) Sample metrics (guide)
Fill after running the commands above:

| Scenario | Cmd | RPS | p50 | p95 | Errors | Backend split |
| --- | --- | --- | --- | --- | --- | --- |
| GET /api/v1/health (400 @40c) | ab | _ | _ | _ | _ | ~200/100/100 (main/ro1/ro2) |
| POST /api/v1/events (50 @10c) | ab | _ | _ | _ | _ | 100% eventity-app:5001 |
| GET /mirror/api/v1/health (100 @20c) | ab | _ | _ | _ | _ | 100% eventity-app-mirror:5004 |

## 8) Cleanup
```sh
docker-compose -f docker-compose.nginx.yml down -v
```
