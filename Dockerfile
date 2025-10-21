FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

RUN apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y curl tshark && \
    apt-get clean

RUN curl -o allure-2.20.1.tgz -Ls https://github.com/allure-framework/allure2/releases/download/2.20.1/allure-2.20.1.tgz \
    && tar -zxvf allure-2.20.1.tgz -C /opt/ \
    && ln -s /opt/allure-2.20.1/bin/allure /usr/bin/allure

RUN dotnet tool install --global dotnet-ef --version 7.0.0

COPY ./src/ .

RUN dotnet nuget locals all --clear

RUN dotnet restore
RUN dotnet build --no-restore

CMD ["sh", "-c", "\
echo 'Starting network capture...' && \
tshark -i any -f 'port 5432' -w /src/db-traffic.pcapng & \
TSHARK_PID=$! && \
sleep 3 && \
echo 'Running tests with Allure...' && \
dotnet test Eventity.Tests.Unit/ --logger trx && \
dotnet test Eventity.Tests.Integration/ --logger trx && \
dotnet test Eventity.Tests.E2E/ --logger trx && \
echo 'Stopping network capture...' && \
kill $TSHARK_PID && \
sleep 2 && \
echo 'Generating Allure report...' && \
allure generate allure-results -o allure-report --clean && \
echo 'Tests completed'\
"]