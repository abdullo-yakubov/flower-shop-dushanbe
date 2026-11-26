# 🌷 Цветочный — интернет-магазин цветов (Душанбе)

Витрина букетов с доставкой по Душанбе: каталог, корзина, оформление заказа, личный кабинет,
программа лояльности. Три языка (русский, English, тоҷикӣ), светлая и тёмная тема, mobile-first.

| Часть | Стек |
| --- | --- |
| [flower-shop/](flower-shop/) | React 19 · Vite 8 · TypeScript · Tailwind 4 · zustand · TanStack Query · i18next |
| [flower-shop-backend/](flower-shop-backend/) | .NET 10 · ASP.NET Core · EF Core 9 + PostgreSQL 16 · FluentValidation · JWT · SkiaSharp |

## Быстрый старт

**Docker** (всё сразу):

```powershell
Copy-Item .env.example .env   # заполните пароли и JWT_SECRET_KEY (32+ символа)
./start-docker.ps1            # магазин: http://localhost:5173, API: http://localhost:5000
```

**Локально** (нужны Node 22, .NET 10 SDK и PostgreSQL на `localhost:5432`):

```powershell
# строка подключения и Jwt:SecretKey — в flower-shop-backend/src/FlowerShop.API/appsettings.Development.json
./start-local.ps1
```

**Только фронтенд на моках** — `cd flower-shop; npm install; npm run dev` (оставьте `VITE_API_URL` пустым).
Тестовый OTP-код: `123456`, промокод: `WELCOME20`.

## Проверки

```bash
cd flower-shop && npx tsc -b --noEmit && npx eslint . --max-warnings 0 && npm test && npm run build
cd flower-shop-backend && dotnet build FlowerShop.slnx -c Release && dotnet test FlowerShop.slnx -c Release
```

Те же шаги выполняет GitHub Actions ([.github/workflows/ci.yml](.github/workflows/ci.yml)) на каждый push в `main`.

## Документация

- [ТЗ.md](ТЗ.md) — техническое задание: роли, функции, API, доменная модель.
- [DEVELOPMENT.md](DEVELOPMENT.md) — что сделано и что осталось.
- [CLAUDE.md](CLAUDE.md) — краткая памятка по архитектуре и бизнес-правилам.
