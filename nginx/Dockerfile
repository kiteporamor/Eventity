ARG NGINX_VERSION=1.27.5
ARG HEADERS_MORE_VERSION=0.37

FROM alpine:3.20 AS build
ARG NGINX_VERSION
ARG HEADERS_MORE_VERSION
RUN apk add --no-cache build-base pcre-dev zlib-dev openssl-dev curl
RUN curl -sL http://nginx.org/download/nginx-${NGINX_VERSION}.tar.gz -o /tmp/nginx.tar.gz \
    && tar -xzf /tmp/nginx.tar.gz -C /tmp \
    && curl -sL https://github.com/openresty/headers-more-nginx-module/archive/refs/tags/v${HEADERS_MORE_VERSION}.tar.gz \
        -o /tmp/headers-more.tar.gz \
    && tar -xzf /tmp/headers-more.tar.gz -C /tmp \
    && cd /tmp/nginx-${NGINX_VERSION} \
    && ./configure --with-compat --add-dynamic-module=/tmp/headers-more-nginx-module-${HEADERS_MORE_VERSION} \
    && make modules

FROM nginx:${NGINX_VERSION}-alpine
ARG NGINX_VERSION
RUN apk add --no-cache apache2-utils curl
COPY --from=build /tmp/nginx-${NGINX_VERSION}/objs/ngx_http_headers_more_filter_module.so /usr/lib/nginx/modules/
