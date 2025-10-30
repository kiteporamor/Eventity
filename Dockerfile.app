FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY ./src/Eventity.Web/bin/Release/net6.0/publish/ .

ENV ASPNETCORE_URLS=http://*:5001
EXPOSE 5001

ENTRYPOINT ["dotnet", "Eventity.Web.dll"]