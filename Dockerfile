FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet build --no-restore -c Release

CMD ["dotnet", "test", "--logger:trx", "--results-directory", "/TestResults", "--configuration", "Release"]