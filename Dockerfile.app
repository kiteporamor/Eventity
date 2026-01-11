FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /src

COPY ./src/ .

RUN dotnet restore
RUN dotnet publish Eventity.Web -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=builder /app/publish .

ENV ASPNETCORE_URLS=http://*:5001
EXPOSE 5001

ENTRYPOINT ["./Eventity.Web"]