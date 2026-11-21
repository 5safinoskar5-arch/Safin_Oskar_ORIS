# Star Wars: Unlimited DB

Учебный веб-проект (семестровая работа по ОРИС): база данных карточных коллекционных игр
Star Wars: Unlimited — карты, выпуски (сеты), комментарии и рейтинги пользователей, админ-панель.

## Стек

- **.NET 8 / C#** — весь бэкенд без ASP.NET Core.
- **`HttpListener`** — собственный HTTP-сервер (`SwU.HttpFramework`) с цепочкой обработчиков
  (статика → маршруты → 404).
- **Собственный ORM поверх `Npgsql`** (`SwU.Orm`): атрибуты маппинга, `SqlBuilder`,
  только параметризованные SQL-запросы.
- **PostgreSQL 16** — хранение данных, схема и тестовые данные в `sql/`.
- **HTML5 / CSS3 / Vanilla JS** — фронтенд без фреймворков; локальный Quill.js только для
  WYSIWYG-редактирования текста способностей карт в админке.
- **Docker / Docker Compose** — запуск одной командой.

## Архитектура проектов

| Проект | Назначение |
| --- | --- |
| `SwU.HttpFramework` | HTTP-сервер на `HttpListener`, маршрутизация по атрибутам, шаблонизатор (`SimpleTemplateEngine`), статические файлы, pipeline (Chain of Responsibility). |
| `SwU.Orm` | Мини-ORM: `IDbContext`, `PostgresDbContext`, `SqlBuilder`, атрибуты `DbTable`/`DbColumn`/`DbPrimaryKey`. |
| `SwU.WebApp` | Приложение: контроллеры, репозитории, сервисы (аутентификация, валидация, HTML-санитайзер), `Views/`, `wwwroot/`, точка входа `Program.cs`. |
| `SwU.Tests` | Модульные тесты (MSTest): шаблонизатор, санитайзер, SQL-билдер, валидация. |

## Запуск через Docker (рекомендуется)

```bash
docker compose up -d --build
```

Что происходит:

1. Поднимается контейнер `db` (`postgres:16-alpine`) и при первой инициализации выполняет
   `sql/01_schema.sql` и `sql/02_seed.sql`.
2. `web` ждёт healthcheck базы (`pg_isready`), затем собирается и стартует на порту `8080`.

Открыть в браузере: **http://localhost:8080/**

Тестовые аккаунты (из `sql/02_seed.sql`):

| Логин | Пароль | Роль |
| --- | --- | --- |
| `admin` | `Admin123!` | Admin (доступ к `/admin`) |
| `player` | `Player123!` | User |

Полезные команды:

```bash
docker compose logs -f web    # логи приложения
docker compose down           # остановить
docker compose down -v        # остановить и удалить данные БД (init-скрипты выполнятся заново)
```

## Запуск без Docker

Нужны .NET 8 SDK и локальный PostgreSQL с базой `swu` (применить `sql/01_schema.sql`, затем `sql/02_seed.sql`).

```bash
# PowerShell
$env:DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=swu;Username=postgres;Password=postgres"
dotnet run --project SwU.WebApp
```

Переменные окружения:

- `DB_CONNECTION_STRING` — строка подключения к PostgreSQL (обязательна);
- `HTTP_PREFIX` — адрес прослушивания, по умолчанию `http://localhost:8080/`
  (в контейнере обязательно `http://+:8080/`).

## Тесты

```bash
dotnet build SwUnlimited.sln
dotnet test SwUnlimited.sln
```
