#!/bin/bash

docker-compose -f docker-compose.nginx.yml up --build

# docker-compose -f docker-compose.nginx.yml down
# docker-compose -f docker-compose.nginx.yml ps