# flower-shop — клиентская часть

React 19, Vite, TypeScript, Tailwind CSS 4. Структура по Feature-Sliced Design:
`app / pages / features / entities / shared`.

```bash
npm install
npm run dev      # http://localhost:5173
npm test
npm run build
```

Если `VITE_API_URL` в `.env` пустой, магазин работает на демо-данных
(`src/entities/product/mock.ts`). Код входа в демо-режиме — `123456`.

Иконки — [lucide-react](https://lucide.dev), логотип — `src/shared/ui/Logo.tsx`
и `public/favicon.svg`. Переводы — `src/shared/lib/i18n` (ru, en, tg).

Общее описание проекта и запуск вместе с API — в [README](../README.md) в корне.
