# Повелитель Токио - сервер

Вики по настольной игре «Повелитель Токио» на собственном HTTP-сервере (.NET 10, без ASP.NET). Сервер, роутинг, ORM и шаблонизатор написаны с нуля, данные хранятся в PostgreSQL.

## Требования

* .NET 10 SDK
* PostgreSQL 16 - или просто Docker, тогда базу поднимать отдельно не нужно

## База данных

В папке `sql/` лежат два скрипта:

* `01\_schema.sql` - таблицы (`monsters`, `cards`, `items`, `rules`, `strategies`, `users`);
* `02\_seed.sql` - тестовые данные и учётка администратора.

**Через Docker:**

```
docker-compose up --build
```

При первом старте Postgres сам выполнит оба скрипта (папка `sql/` примонтирована в `/docker-entrypoint-initdb.d`). Сайт откроется на http://localhost:8888.

**Вручную:**

1. Создать базу и пользователя:

```
psql -U postgres -c "CREATE USER pt\_user WITH PASSWORD 'pt\_pass';"
psql -U postgres -c "CREATE DATABASE povelitel\_tokio OWNER pt\_user;"
```

2. Накатить схему и данные:

```
psql -U pt\_user -d povelitel\_tokio -f sql/01\_schema.sql
psql -U pt\_user -d povelitel\_tokio -f sql/02\_seed.sql
```

Строка подключения хранится в `MiniHttpServer/settings.json`, по умолчанию:

```
Host=localhost;Port=5432;Database=povelitel\_tokio;Username=pt\_user;Password=pt\_pass;Include Error Detail=true
```

Если параметры другие — правим `settings.json` или задаём переменную окружения `PT\_CONNECTION\_STRING` (также есть `PT\_SERVER\_HOST` и `PT\_SERVER\_PORT`).

## Запуск

```
dotnet run --project MiniHttpServer/MiniHttpServer.csproj
```

Сервер поднимается на http://127.0.0.1:8888. Чтобы остановить — в консоли вводим `stop`.

## Вход и работа с админкой

Сайт открыт для всех без регистрации — карты, предметы, правила и стратегии можно смотреть свободно. Кнопка входа в шапке подписана «Вход для админа»:

* логин: `admin`
* пароль: `admin123`

После входа сверху появляется плашка со ссылкой на админ-панель (`/admin/index`). В ней можно:

* **Карты** — добавлять, редактировать и удалять (название, тип, описание, картинка);
* **Предметы** — то же самое с полем «количество»;
* **Правила** и **Стратегии** — редактируются в визуальном редакторе (Quill), удаляются тоже из панели.

Картинки загружаются прямо в форме (сжимаются до 600 px по длинной стороне) и сохраняются в `static/assets/images/uploads/`. Удаление работает без перезагрузки страницы — строки исчезают сразу.

## Тесты

```
dotnet test
```

Покрыты ORM и шаблонизатор.

## Структура проекта

* `MiniHttpServer/` — сам сайт: контроллеры, шаблоны (`Templates/`), статика (`static/assets/`), `settings.json`.
* `MiniHttpServer.Framework/` — HTTP-сервер на `HttpListener`, роутинг, конфигурация, аутентификация по сессиям.
* `MiniHttpServer.ORM/` — мини-ORM для PostgreSQL.
* `MiniHttpServer.HtmlTemplating/` — шаблонизатор: `$foreach`, `$if`, `${...}`.
* `sql/` — схема и сид.

