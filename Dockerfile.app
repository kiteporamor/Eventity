FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /src

COPY ./src/ .

RUN dotnet restore
RUN dotnet build --no-restore

ENV ASPNETCORE_URLS=http://*:5001
EXPOSE 5001

CMD ["dotnet", "run", "--project", "Eventity.Web", "--urls", "http://0.0.0.0:5001"]