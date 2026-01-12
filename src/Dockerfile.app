FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY *.sln .
COPY Eventity.Application/*.csproj Eventity.Application/
COPY Eventity.Domain/*.csproj Eventity.Domain/
COPY Eventity.DataAccess/*.csproj Eventity.DataAccess/
COPY Eventity.Web/*.csproj Eventity.Web/
COPY Eventity.Tests.Unit/*.csproj Eventity.Tests.Unit/
COPY Eventity.Tests.Integration/*.csproj Eventity.Tests.Integration/

RUN dotnet restore

COPY . .

WORKDIR /src/Eventity.Web
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eventity.Web.dll"]