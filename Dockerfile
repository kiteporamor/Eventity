FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

RUN apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y curl tshark openjdk-11-jre && \
    apt-get clean

RUN curl -o allure-2.20.1.tgz -Ls https://github.com/allure-framework/allure2/releases/download/2.20.1/allure-2.20.1.tgz \
    && tar -zxvf allure-2.20.1.tgz -C /opt/ \
    && ln -s /opt/allure-2.20.1/bin/allure /usr/bin/allure

ENV JAVA_HOME=/usr/lib/jvm/java-11-openjdk-amd64
ENV PATH="$JAVA_HOME/bin:${PATH}"

RUN dotnet tool install --global dotnet-ef --version 7.0.0

COPY ./src/ .

RUN dotnet nuget locals all --clear
RUN dotnet restore
RUN dotnet build --no-restore

CMD ["sh", "-c", "\
set -e && \
echo 'Setting up directories and permissions...' && \
mkdir -p /src/allure-results /src/allure-report && \
chmod -R 755 /src/allure-results /src/allure-report && \
echo 'Starting network capture...' && \
tshark -i any -f 'tcp port 5432' -w /src/allure-results/db-traffic.pcapng -q & \
TSHARK_PID=$! && \
sleep 5 && \
echo 'Running tests...' && \
dotnet test Eventity.Tests.Unit/ --logger trx && \
dotnet test Eventity.Tests.Integration/ --logger trx && \
dotnet test Eventity.Tests.E2E/ --logger trx && \
echo 'Stopping network capture...' && \
kill $TSHARK_PID && \
sleep 2 && \
echo 'Generating Allure report...' && \
chmod -R 755 /src/allure-results && \
allure generate allure-results -o allure-report --clean && \
chmod -R 755 /src/allure-report && \
echo 'All operations completed successfully'\
"]