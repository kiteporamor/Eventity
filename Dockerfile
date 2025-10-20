FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y curl
RUN curl -o allure-2.20.1.tgz -Ls https://github.com/allure-framework/allure2/releases/download/2.20.1/allure-2.20.1.tgz \
    && tar -zxvf allure-2.20.1.tgz -C /opt/ \
    && ln -s /opt/allure-2.20.1/bin/allure /usr/bin/allure

COPY ./src/ .
RUN dotnet restore
RUN dotnet build --no-restore

# CMD ["sh", "-c", "\
# dotnet test Eventity.Tests.Unit/ --logger:trx --results-directory:/TestResults && \
# dotnet test Eventity.Tests.Integration/ --logger:trx --results-directory:/TestResults && \
# dotnet test Eventity.Tests.E2E/ --logger:trx --results-directory:/TestResults && \
# allure generate /TestResults -o /allure-report --clean\
# "]

CMD ["sh", "-c", "\
dotnet test Eventity.Tests.Unit/ --logger trx && \
dotnet test Eventity.Tests.Integration/ --logger trx && \
echo 'E2E tests skipped - project not found'\
"]