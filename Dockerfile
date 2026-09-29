FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Account.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish Account.csproj -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
# Run as the built-in non-root user provided by the aspnet image.
USER $APP_UID
ENTRYPOINT ["dotnet", "Account.dll"]
