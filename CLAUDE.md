# CLAUDE.md — Flower Shop «Цветочный» (Душанбе)

Интернет-магазин цветов с доставкой по Душанбе. Монорепозиторий: React-фронтенд + .NET 10 API + PostgreSQL 16.
Полное ТЗ — [ТЗ.md](ТЗ.md). План развития — [DEVELOPMENT.md](DEVELOPMENT.md).

## Правила работы

- Общение, комментарии к коммитам и документация — **на русском**. Идентификаторы кода — на английском.
- **Только ветка `main`.** Других веток (локальных и на `origin`) не создавать; коммитить и пушить в `main`.
- Перед коммитом обязательно зелёные: `tsc`, `eslint --max-warnings 0`, `vitest`, `dotnet build -c Release`, `dotnet test`.
- Секреты не коммитить: `.env`, `appsettings.Development.json`, `.claude/` в `.gitignore`.

## Команды

```bash
# Frontend (flower-shop/)
npm install
npm run dev                 # http://localhost:5173 (VITE_API_URL пустой → мок-данные)
npx tsc -b --noEmit && npx eslint . --max-warnings 0
npm test                    # vitest
npm run build && npm run check:bundle   # бюджет 350 КБ gzip
npm run check:consistency   # сверка конфигов/i18n

# Backend (flower-shop-backend/)
dotnet build FlowerShop.slnx -c Release
dotnet test FlowerShop.slnx -c Release
dotnet list FlowerShop.slnx package --vulnerable --include-transitive

# Всё вместе
./start-local.ps1           # API :5000 + фронт :5173 (нужен локальный PostgreSQL)
./start-docker.ps1          # docker compose (нужен .env, см. .env.example)
```

## Архитектура

**Frontend** `flower-shop/src` — Feature-Sliced Design: `app / pages / features / entities / shared`.
React 19, Vite 8, TS 6, Tailwind 4 (токены в `index.css` через `@theme`), zustand+persist, TanStack Query, i18next (ru/en/tg).
API-клиент: `shared/lib/api/client.ts` (JWT + авто-refresh). Если `VITE_API_URL` пуст — всё работает на моках.

**Backend** `flower-shop-backend/src` — Clean Architecture:
- `Domain` — сущности, enum, `Constants/` (`OrderConstants`, `LoyaltyConstants`, `EnumExtensions.ToApiString()`).
- `Application` — DTO, FluentValidation-валидаторы.
- `Infrastructure` — `AppDbContext` (EF Core + Npgsql), миграции, сервисы.
- `API` — контроллеры (`/api/v1/...`), `GlobalExceptionHandlingMiddleware`, `Program.cs`.
- `tests/FlowerShop.Tests` — xUnit + EF InMemory (`TestDb` подменяет `JsonDocument` на строку).

## Бизнес-логика (источник истины)

- **Цена заказа считает только `PricingService.QuoteAsync`** (бэкенд). Фронт показывает предпросмотр;
  `POST /orders/quote` возвращает серверный расчёт, `POST /orders` списывает ровно его.
- Формула: `Total = товары + аддоны + доставка − промо − баллы` (не ниже 0).
  - Товар: `size.Price`, для `Single` — `StemPrice × StemCount` (только из `StemOptions`).
  - Праздничная наценка (`HolidayPricing`) — на базовую цену живых цветов (или всех при `AppliesTo=all`), не на корзинку.
  - Корзинка `+80` с (`OrderConstants.BasketSurcharge` ↔ `packagingConfig.ts`).
  - Доставка: зона или настройки `default_delivery_fee`/`free_delivery_threshold` (40 / 1500 с, от суммы товаров).
  - Промокод: % от суммы товаров либо фикс (не больше суммы товаров); сроки, `MaxUses`, `MinOrder`.
  - Баллы: 1 балл = 1 с; минимум 50; не больше 30 % от суммы к оплате и не больше баланса. Начисление `Total / 20`.
- Создание заказа — одна транзакция + `pg_advisory_xact_lock` для номера `#000123`.
- Отмена (только статус `accepted`): возврат списанных баллов, снятие начисленных, `UsedCount--` промокода, откат статистики пользователя.
- Константы продублированы на фронте и должны совпадать: `shared/config/appConfig.ts`, `packagingConfig.ts`,
  `deliveryConfig.ts` (слоты с длинным тире `09:00–12:00`; бэкенд принимает и дефис), `features/loyalty/store.ts`.

## Контракт API

- Ответ: `{ success, data }` или `{ success:false, error:{ code, message, details? } }` — в т.ч. для ошибок валидации.
- Enum в JSON — `snake_case` через `ToApiString()` (`photo_ready`, `courier_en_route`, `admin_adjust`).
- Коды ошибок `AppException`: `NOT_FOUND`→404, `UNAUTHORIZED`→401, `FORBIDDEN`→403, `CONFLICT`→409, `RATE_LIMIT`→429, прочие→400.
- Rate limit по IP: глобально 100/мин, OTP 10/час.

## Подводные камни окружения (Windows)

- Если API запущен, `dotnet build` в Debug падает на заблокированных DLL → собирайте `-c Release` или `-o <другая папка>`.
- У пользователя БД `flowershop` нет права `CREATEDB` — временную базу для проверок не создать; используйте тесты.
- `python` не установлен (заглушка Microsoft Store) — правки через Edit/sed.
- `core.autocrlf=true`: Prettier настроен на `endOfLine: auto`.
- Изображения: SkiaSharp (MIT). **Не возвращать ImageSharp** — 3.1.x уязвим, 3.2+ требует платную лицензию для Release.

## Технический долг

- Eskiz SMS не реализован (`EskizSmsService` — заглушка), в проде использовать `SMS_PROVIDER=mock` нельзя.
- Платёжные шлюзы (Душанбе Сити, Alif) не подключены — `IsPaid` не выставляется.
- Нет idempotency-ключа у `POST /orders` (защита от двойной отправки только на клиенте).
- Чекаут фронта ещё не вызывает `/orders/quote` для показа серверной суммы.
