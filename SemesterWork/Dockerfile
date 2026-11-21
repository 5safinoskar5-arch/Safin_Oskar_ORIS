# ---------- Этап 1: сборка ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Сначала копируем только файлы проектов: слой dotnet restore будет кэшироваться,
# пока не изменились .csproj / .sln (SwU.Tests в образ не попадает, см. .dockerignore).
COPY SwUnlimited.sln ./
COPY SwU.HttpFramework/SwU.HttpFramework.csproj SwU.HttpFramework/
COPY SwU.Orm/SwU.Orm.csproj SwU.Orm/
COPY SwU.WebApp/SwU.WebApp.csproj SwU.WebApp/

RUN dotnet restore SwU.WebApp/SwU.WebApp.csproj

# Теперь копируем исходники и публикуем приложение
COPY . .
RUN dotnet publish SwU.WebApp/SwU.WebApp.csproj -c Release -o /app/publish --no-restore

# Страховка: без Views и wwwroot приложение не стартует, поэтому проверяем их наличие
RUN test -d /app/publish/Views && test -d /app/publish/wwwroot

# ---------- Этап 2: запуск ----------
# Нужен только runtime (HttpListener входит в Microsoft.NETCore.App, ASP.NET Core не используется)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

# Значения по умолчанию; docker-compose.yml переопределяет их при запуске
ENV HTTP_PREFIX=http://+:8080/
ENV DB_CONNECTION_STRING=Host=db;Port=5432;Database=swu;Username=postgres;Password=postgres

EXPOSE 8080
ENTRYPOINT ["dotnet", "SwU.WebApp.dll"]
