# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:3.1

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Bigs3cRet
ENV MSSQL_PID=Express
ENV connectionString="Server=127.0.0.1; Initial Catalog=databaseName; User ID=sa; Password=Bigs3cRet"
ENV ASPNETCORE_URLS=https://localhost:44363 
ENV ASPNETCORE_ENVIRONMENT=local


COPY bin/Release/netcoreapp3.1/publish/ App/
WORKDIR /App

#RUN dotnet user-secrets set "ConnectionStrings:ApplicationDb" "Server=localhost; Initial Catalog=databaseName; User ID=sa; Password=Bigs3cRet"
#RUN dotnet user-secrets set "SENDGRID_PORT" "2500"
#RUN dotnet user-secrets set "SENDGRID_HOST" "127.0.0.1"

ENTRYPOINT ["dotnet", "EAVFW.MainApp.dll", "--property:connectionString=\"Server=127.0.0.1; Initial Catalog=databaseName; User ID=sa; Password=Bigs3cRet\""]