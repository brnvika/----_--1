# Этап сборки: восстанавливаем зависимости и публикуем приложение.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Wishlist.Api.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish Wishlist.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# Этап запуска: в финальный образ попадает только опубликованное приложение.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install --no-install-recommends --yes curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Wishlist.Api.dll"]
