import type { Product } from './types'

// ─── MARKET AUDIT NOTE (updated 2026-07) ─────────────────────────────────────
// Prices anchored to LIVE competitor research (July 2026):
//   • gulianor.tj:   авторские букеты 560–1 680 сом, конструктор от 550 сом
//   • flowerstyle.tj: пионовидные/коробки 450–1 800 сом, авторские от 1 500 сом
//   • roza.tj:       51 роза ~350 сом, 101 роза 280–650 сом, тюльпан от 10 сом/шт
//   • magnit.tj:     7 тюльпанов 209 сом, 21 тюльпан 589 сом
//
// Stem cost benchmarks (local Dushanbe market):
//   • Местные розы: 5–12 сом/стебель
//   • Голландские/импортные розы: 20–40 сом/стебель
//   • Тюльпаны: 8–15 сом/стебель
//   • Пионы: 25–50 сом/стебель
//   • Хризантемы, ромашки: 8–20 сом/стебель
//   • Экзотика (стрелиция, протея): 100–150 сом/стебель
//   • Сборка/упаковка: +80–300 сом в зависимости от объёма
// ⚠️  Перед запуском в продакшн — сверить с 2–3 текущими поставщиками.
// ─────────────────────────────────────────────────────────────────────────────

const P = (id: number) =>
  `https://images.pexels.com/photos/${id}/pexels-photo-${id}.jpeg?auto=compress&cs=tinysrgb&w=600&h=800&fit=crop`

// Every product gets its own unique pair of content-matched photos — no photo ID repeats across the catalog.
const IMG = {
  redRoses: [P(19269253), P(754273)],
  davidAustin: [P(17295683), P(38396369)],
  pinkRoseMix: [P(30891127), P(36117537)],
  whiteRoseMono: [P(15178896), P(30325205)],
  redRoseSingle: [P(13977740), P(11281418)],
  roseGypsophila: [P(6955468), P(34691226)],
  pinkPeony: [P(33955699), P(38006062)],
  whitePeony: [P(8345863), P(12222145)],
  peonyHatbox: [P(3392982), P(14590333)],
  peonySingle: [P(17457660), P(36399726)],
  tulipMix51: [P(27094717), P(7353313)],
  tulipSinglePurple: [P(16111756), P(11141950)],
  yellowTulip: [P(11955430), P(37296942)],
  tulipBox: [P(8644153), P(30724309)],
  springMix: [P(37423023), P(33791888)],
  meadowFlowers: [P(7831098), P(12774934)],
  driedField: [P(6732262), P(4388365)],
  gardenMix: [P(34789794), P(36857105)],
  daisies: [P(13555393), P(10583573)],
  whiteChrysanthemum: [P(20659761), P(16292522)],
  pampas: [P(11435411), P(9951150)],
  whiteEustoma: [P(33485942), P(18386639)],
  exotic: [P(30495250), P(7626651)],
  hydrangea: [P(8723269), P(36186130)],
  funeral: [P(3987856), P(18637247)],
}

export const MOCK_PRODUCTS: Product[] = [
  // ── Розы (букеты) ─────────────────────────────────────────────────────────
  {
    // #1 — 21 роза Фридом: рынок ~25–30 сом/стебель × 21 + сборка 100 ≈ 625 сом
    // Конкуренты: roza.tj 51 роза ~350 сом → 21 роза ~145 сом (масс-маркет без упак.)
    // Guli Anor авторский 560–620 сом. Ставим 590 сом — конкурентная серединная цена.
    id: '1',
    slug: 'krasnye-rozy-21',
    name: 'Классические красные розы',
    composition: '21 роза сорта Фридом, гипсофила, декоративная зелень',
    images: [IMG.redRoses[0], IMG.redRoses[1]],
    price: 590,
    oldPrice: 980,
    discount: 40,
    rating: 4.9,
    reviewCount: 87,
    occasions: ['birthday', 'wedding'],
    flowerTypes: ['rose'],
    colors: ['red'],
    isHit: true,
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 380 },
      { id: 'm', label: 'M', price: 590, oldPrice: 980 },
      { id: 'l', label: 'L', price: 870 },
    ],
  },
  {
    // #2 — 15 роз Дэвид Остин: премиум импорт ~60–80 сом/стебель × 15 + сборка 200 ≈ 1100–1400 сом
    // Guli Anor «Императорский Сад» (премиум) = 1450 сом. Ставим 1200 сом.
    id: '2',
    slug: 'roza-devid-ostin',
    name: 'Роза Дэвид Остин',
    composition: '15 роз Дэвид Остин, эвкалипт, питтоспорум',
    images: [IMG.davidAustin[0], IMG.davidAustin[1]],
    price: 1200,
    rating: 5.0,
    reviewCount: 42,
    occasions: ['wedding', 'birthday'],
    flowerTypes: ['rose'],
    colors: ['pink', 'white'],
    isNew: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 780 },
      { id: 'm', label: 'M', price: 1200 },
      { id: 'l', label: 'L', price: 1700 },
    ],
  },
  {
    id: '3',
    slug: 'rozy-miks-rozovye',
    name: 'Розовый микс роз',
    composition: '25 роз разных оттенков розового, лента, упаковка',
    images: [IMG.pinkRoseMix[0], IMG.pinkRoseMix[1]],
    // #3 — 25 роз розовый микс: ~25 сом/стебель × 25 + сборка 130 ≈ 755 сом
    // Guli Anor «Розовая Линия» = 560 сом (меньше роз). Ставим 720 сом.
    price: 720,
    rating: 4.7,
    reviewCount: 63,
    occasions: ['birthday', 'mom'],
    flowerTypes: ['rose'],
    colors: ['pink'],
    isNew: true,
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 460 },
      { id: 'm', label: 'M', price: 720 },
      { id: 'l', label: 'L', price: 1050 },
    ],
  },
  {
    id: '4',
    slug: 'belye-rozy-mono',
    name: 'Монобукет белых роз',
    composition: '19 белых роз Аваланш, флористическая бумага',
    images: [IMG.whiteRoseMono[0], IMG.whiteRoseMono[1]],
    // #4 — 19 роз Аваланш: голландский сорт ~35 сом/стебель × 19 + сборка 120 ≈ 785 сом
    // Guli Anor «Белая Поэзия» (моно белые) = 740 сом. Ставим 760 сом.
    price: 760,
    rating: 4.8,
    reviewCount: 34,
    occasions: ['wedding', 'sorry'],
    flowerTypes: ['rose'],
    colors: ['white'],
    isNew: true,
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 490 },
      { id: 'm', label: 'M', price: 760 },
      { id: 'l', label: 'L', price: 1100 },
    ],
  },
  // ── Розы (штучные) ────────────────────────────────────────────────────────
  {
    id: '5',
    slug: 'alye-rozy-11',
    name: '11 алых роз',
    composition: 'Розы Red Naomi — поштучный выбор количества',
    images: [IMG.redRoseSingle[0], IMG.redRoseSingle[1]],
    // #5 — Red Naomi поштучно: рынок 20–40 сом/стебель (голландский импорт)
    // roza.tj: 51 роза ~350 сом = ~7 сом/шт (масс-маркет). Розничная наценка × 2–3.
    // Guli Anor: бутик-цена ~35–40 сом/шт. Ставим 28 сом/стебель — средний розничный.
    // M-size aggregate = 11 × 28 = 308
    price: 28,
    oldPrice: 45,
    discount: 38,
    rating: 4.6,
    reviewCount: 119,
    occasions: ['birthday', 'sorry', 'mom'],
    flowerTypes: ['rose'],
    colors: ['red'],
    isHit: true,
    inStock: true,
    deliverToday: true,
    kind: 'single',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    stemPrice: 28,
    stemOptions: [1, 5, 11, 25, 51, 101],
    sizes: [{ id: 'm', label: 'M', price: 308 }],
  },
  {
    id: '6',
    slug: 'rozy-gipsofila',
    name: 'Розы с гипсофилой',
    composition: '15 роз Эквадор, облако гипсофилы, рускус',
    images: [IMG.roseGypsophila[0], IMG.roseGypsophila[1]],
    // #6 — 15 роз с гипсофилой: ~28 сом × 15 + гипсофила 80 + сборка 80 ≈ 580 сом
    // Guli Anor «Розовая Линия» (похожий формат) = 560 сом. Ставим 580 сом.
    price: 580,
    rating: 4.8,
    reviewCount: 55,
    occasions: ['wedding', 'birthday'],
    flowerTypes: ['rose'],
    colors: ['red', 'pink'],
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 375 },
      { id: 'm', label: 'M', price: 580 },
      { id: 'l', label: 'L', price: 850 },
    ],
  },

  // ── Пионы ─────────────────────────────────────────────────────────────────
  {
    id: '7',
    slug: 'piony-nezhno-rozovye',
    name: 'Нежные розовые пионы',
    composition: '15 пионов сорта Сара Бернар, листья эвкалипта',
    images: [IMG.pinkPeony[0], IMG.pinkPeony[1]],
    // #7 — 15 пионов Сара Бернар: рынок ~35–50 сом/стебель × 15 + сборка 150 ≈ 675–900 сом
    // Flower Style пионовидные коробки от 450 сом. Ставим 780 сом.
    price: 780,
    oldPrice: 1200,
    discount: 35,
    rating: 4.9,
    reviewCount: 78,
    occasions: ['birthday', 'wedding', 'mom'],
    flowerTypes: ['peony'],
    colors: ['pink'],
    isHit: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 500 },
      { id: 'm', label: 'M', price: 780, oldPrice: 1200 },
      { id: 'l', label: 'L', price: 1120 },
    ],
  },
  {
    id: '8',
    slug: 'belye-piony',
    name: 'Белоснежные пионы',
    composition: '11 белых пионов Duchesse de Nemours, зелень смилакс',
    images: [IMG.whitePeony[0], IMG.whitePeony[1]],
    // #8 — 11 белых пионов: ~40 сом/стебель × 11 + сборка 120 ≈ 560 сом
    // Ставим 580 сом — конкурентная цена.
    price: 580,
    rating: 5.0,
    reviewCount: 31,
    occasions: ['wedding', 'birthday'],
    flowerTypes: ['peony'],
    colors: ['white'],
    isNew: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 375 },
      { id: 'm', label: 'M', price: 580 },
      { id: 'l', label: 'L', price: 840 },
    ],
  },
  {
    id: '9',
    slug: 'piony-korobka',
    name: 'Пионы в шляпной коробке',
    composition: '20 пионов, мох, сухоцветы, шляпная коробка',
    images: [IMG.peonyHatbox[0], IMG.peonyHatbox[1]],
    // #9 — 20 пионов в шляпной коробке: ~40 сом × 20 + коробка 250 ≈ 1050 сом
    // Flower Style коробки от 450–1800 сом (разный объём). Ставим 1050 сом.
    price: 1050,
    rating: 4.9,
    reviewCount: 22,
    occasions: ['birthday', 'wedding'],
    flowerTypes: ['peony'],
    colors: ['pink', 'white'],
    isNew: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 680 },
      { id: 'm', label: 'M', price: 1050 },
      { id: 'l', label: 'L', price: 1500 },
    ],
  },
  // ── Пионы (штучные) ───────────────────────────────────────────────────────
  {
    id: '10',
    slug: 'piony-poshtuchno',
    name: 'Пионы поштучно',
    composition: 'Пионы с эвкалиптом — выбор количества стеблей',
    images: [IMG.peonySingle[0], IMG.peonySingle[1]],
    // #10 — Пионы поштучно: рынок ~35–50 сом/стебель. Ставим 38 сом/стебель.
    // M-size aggregate = 11 × 38 = 418
    price: 38,
    rating: 4.7,
    reviewCount: 44,
    occasions: ['mom', 'birthday', 'sorry'],
    flowerTypes: ['peony'],
    colors: ['pink'],
    inStock: true,
    deliverToday: false,
    kind: 'single',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    stemPrice: 38,
    stemOptions: [1, 5, 11, 25, 51],
    sizes: [{ id: 'm', label: 'M', price: 418 }],
  },

  // ── Тюльпаны ──────────────────────────────────────────────────────────────
  {
    id: '11',
    slug: 'tyulpany-51',
    name: 'Тюльпаны 51 шт',
    composition: '51 тюльпан разных цветов, упаковка крафт',
    images: [IMG.tulipMix51[0], IMG.tulipMix51[1]],
    // #11 — Тюльпаны 51 шт: 2100 → 1100 (audit: ~20 сом/стебель × 51 + сборка ~80)
    // oldPrice = pre-audit; discount recalculated: (2100-1100)/2100 ≈ 48%
    price: 1100,
    oldPrice: 2100,
    discount: 48,
    rating: 4.8,
    reviewCount: 142,
    occasions: ['birthday', 'mom', 'other'],
    flowerTypes: ['tulip'],
    colors: ['purple', 'pink', 'yellow'],
    isHit: true,
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 530 },
      { id: 'm', label: 'M', price: 1100, oldPrice: 2100 },
      { id: 'l', label: 'L', price: 1800 },
    ],
  },
  {
    id: '12',
    slug: 'tyulpany-poshtuchno',
    name: 'Тюльпаны поштучно',
    composition: 'Тюльпаны Purple Prince — выбор количества стеблей',
    images: [IMG.tulipSinglePurple[0], IMG.tulipSinglePurple[1]],
    // #12 — Тюльпаны поштучно: рынок 10–15 сом/стебель. Ставим 12 сом/стебель.
    // M-size aggregate = 11 × 12 = 132
    price: 12,
    rating: 4.7,
    reviewCount: 58,
    occasions: ['birthday', 'mom'],
    flowerTypes: ['tulip'],
    colors: ['purple'],
    inStock: true,
    deliverToday: true,
    kind: 'single',
    freshness: 'live',
    packagingOptions: ['no_basket'],
    stemPrice: 12,
    stemOptions: [5, 11, 25, 51, 101],
    sizes: [{ id: 'm', label: 'M', price: 132 }],
  },
  {
    id: '13',
    slug: 'zhyoltye-tyulpany-25',
    name: 'Жёлтые тюльпаны',
    composition: '25 тюльпанов Strong Gold, бумажная упаковка',
    images: [IMG.yellowTulip[0], IMG.yellowTulip[1]],
    // #13 — 25 тюльпанов: ~12 сом × 25 + сборка 90 = 390 сом
    // Magnit.tj: 21 шт = 589 сом (розничная + доставка). Ставим 420 сом (без доставки).
    price: 420,
    rating: 4.6,
    reviewCount: 73,
    occasions: ['birthday', 'other'],
    flowerTypes: ['tulip'],
    colors: ['yellow'],
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 270 },
      { id: 'm', label: 'M', price: 420 },
      { id: 'l', label: 'L', price: 620 },
    ],
  },
  {
    id: '14',
    slug: 'tyulpany-boks',
    name: 'Тюльпаны в боксе',
    composition: '35 тюльпанов, флористический мох, деревянный ящик',
    images: [IMG.tulipBox[0], IMG.tulipBox[1]],
    // #14 — 35 тюльпанов в боксе: ~12 сом × 35 + ящик 300 + мох/сборка 200 ≈ 920 сом
    // Flower Style коробки от 450 сом. Деревянный ящик — доп. ценность. Ставим 950 сом.
    price: 950,
    rating: 4.9,
    reviewCount: 19,
    occasions: ['birthday', 'wedding'],
    flowerTypes: ['tulip'],
    colors: ['purple', 'pink'],
    isNew: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket'],
    sizes: [
      { id: 's', label: 'S', price: 620 },
      { id: 'm', label: 'M', price: 950 },
      { id: 'l', label: 'L', price: 1380 },
    ],
  },

  // ── Весенний/луговой микс ─────────────────────────────────────────────────
  {
    id: '15',
    slug: 'vesenniy-miks',
    name: 'Весенний микс',
    composition: 'Тюльпаны, нарциссы, фрезия, мимоза — 7 видов',
    images: [IMG.springMix[0], IMG.springMix[1]],
    // #15 — Весенний микс: Magnit.tj 7 тюльпанов = 209 сом, 21 = 589 сом
    // Миксовый букет ~25 стеблей × средн. 12 сом + сборка 150 = ~450 сом
    // Guli Anor базовый уровень от 550–560 сом. Ставим 480 сом.
    price: 480,
    oldPrice: 700,
    discount: 31,
    rating: 4.8,
    reviewCount: 96,
    occasions: ['birthday', 'mom', 'other'],
    flowerTypes: ['mixed'],
    colors: ['yellow', 'pink', 'white'],
    isHit: true,
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 310 },
      { id: 'm', label: 'M', price: 480, oldPrice: 700 },
      { id: 'l', label: 'L', price: 700 },
    ],
  },
  {
    id: '16',
    slug: 'lugovye-tsvety',
    name: 'Луговые цветы',
    composition: 'Ромашки, васильки, полевые тюльпаны, сухоцветы, колоски',
    images: [IMG.meadowFlowers[0], IMG.meadowFlowers[1]],
    // #16 — Луговые цветы: полевые/местные 8–12 сом avg × ~30 стеблей + сборка 80 ≈ 320–440 сом
    // Ставим 350 сом — самый бюджетный «живой» вариант в каталоге.
    price: 350,
    rating: 4.6,
    reviewCount: 47,
    occasions: ['other', 'birthday', 'mom'],
    flowerTypes: ['mixed'],
    colors: ['white', 'yellow', 'purple'],
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 225 },
      { id: 'm', label: 'M', price: 350 },
      { id: 'l', label: 'L', price: 510 },
    ],
  },
  // ── Сухоцветы ─────────────────────────────────────────────────────────────
  {
    id: '17',
    slug: 'polevoy-buket-sukhoy',
    name: 'Полевой букет сухоцветов',
    composition: 'Гипсофила, хлопок, эвкалипт, лаванда, сухие травы',
    images: [IMG.driedField[0], IMG.driedField[1]],
    // #17 — Полевой букет сухоцветов: 1300 → 1100 (🟢 close to market; minor correction)
    price: 1100,
    rating: 4.7,
    reviewCount: 61,
    occasions: ['birthday', 'other', 'sorry'],
    flowerTypes: ['mixed'],
    colors: ['white', 'green'],
    isNew: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'dried',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 720 },
      { id: 'm', label: 'M', price: 1100 },
      { id: 'l', label: 'L', price: 1600 },
    ],
  },
  {
    id: '18',
    slug: 'sadovyy-miks',
    name: 'Садовый микс',
    composition: 'Розы, пионы, гортензия, лизиантус — премиум состав',
    images: [IMG.gardenMix[0], IMG.gardenMix[1]],
    // #18 — Садовый микс (роза/пион/гортензия/лизиантус): 2500 → 1700
    // (audit: mixed premium ~60 сом avg × ~20 + сборка ~500)
    price: 1700,
    rating: 4.8,
    reviewCount: 38,
    occasions: ['birthday', 'wedding'],
    flowerTypes: ['mixed'],
    colors: ['pink', 'white', 'purple'],
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 1100 },
      { id: 'm', label: 'M', price: 1700 },
      { id: 'l', label: 'L', price: 2450 },
    ],
  },

  // ── Белые / хризантемы ────────────────────────────────────────────────────
  {
    id: '19',
    slug: 'belye-romashki',
    name: 'Садовые ромашки',
    composition: '50 ромашек, флористическая лента, крафт-упаковка',
    images: [IMG.daisies[0], IMG.daisies[1]],
    // #19 — 50 ромашек: местные 5–8 сом/стебель × 50 + сборка 50 = 300–450 сом
    // Ставим 390 сом — конкурентно для большого объёма.
    price: 390,
    rating: 4.5,
    reviewCount: 82,
    occasions: ['birthday', 'other', 'mom'],
    flowerTypes: ['mixed'],
    colors: ['white', 'yellow'],
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 250 },
      { id: 'm', label: 'M', price: 390 },
      { id: 'l', label: 'L', price: 570 },
    ],
  },
  {
    id: '20',
    slug: 'belye-khrizantemy',
    name: 'Белые хризантемы',
    composition: '25 хризантем Bacardi, декоративная зелень, рафия',
    images: [IMG.whiteChrysanthemum[0], IMG.whiteChrysanthemum[1]],
    // #20 — 25 хризантем Bacardi: ~15 сом/стебель × 25 + зелень 60 + сборка 80 = 515 сом
    // Ставим 520 сом. oldPrice = прежняя завышенная цена.
    price: 520,
    oldPrice: 800,
    discount: 35,
    rating: 4.7,
    reviewCount: 53,
    occasions: ['birthday', 'other', 'funeral'],
    flowerTypes: ['mixed'],
    colors: ['white'],
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 335 },
      { id: 'm', label: 'M', price: 520, oldPrice: 800 },
      { id: 'l', label: 'L', price: 760 },
    ],
  },
  // ── Сухоцветы (монобуке) ──────────────────────────────────────────────────
  {
    id: '21',
    slug: 'sukhye-tsvety-pampas',
    name: 'Пампасная трава и сухоцветы',
    composition: 'Пампасная трава, хлопок, лагурус, гелихризум, лаванда',
    images: [IMG.pampas[0], IMG.pampas[1]],
    // #21 — Пампасная трава и сухоцветы: 2300 → 1900 (🟢 close to market; minor correction)
    price: 1900,
    rating: 4.9,
    reviewCount: 27,
    occasions: ['wedding', 'birthday', 'sorry'],
    flowerTypes: ['mixed'],
    colors: ['white'],
    isNew: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'dried',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 1250 },
      { id: 'm', label: 'M', price: 1900 },
      { id: 'l', label: 'L', price: 2700 },
    ],
  },
  {
    id: '22',
    slug: 'klassicheskiy-belyy',
    name: 'Классический белый',
    composition: '21 эустома, гипсофила, рускус — элегантный моно',
    images: [IMG.whiteEustoma[0], IMG.whiteEustoma[1]],
    // #22 — 21 эустома: ~25 сом × 21 + гипсофила 60 + сборка 80 = 665 сом
    // Guli Anor «Белая Поэзия» = 740 сом. Ставим 680 сом.
    price: 680,
    rating: 4.8,
    reviewCount: 41,
    occasions: ['wedding', 'sorry', 'other'],
    flowerTypes: ['mixed'],
    colors: ['white'],
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 440 },
      { id: 'm', label: 'M', price: 680 },
      { id: 'l', label: 'L', price: 980 },
    ],
  },

  // ── Экзотика ──────────────────────────────────────────────────────────────
  {
    id: '23',
    slug: 'ekzoticheskiy-buket',
    name: 'Экзотический букет',
    composition: 'Протея, стрелиция, антуриум, тропические листья',
    images: [IMG.exotic[0], IMG.exotic[1]],
    // #23 — Протея + стрелиция + антуриум: 3800 → 3400 (🟢 justified; audit: ~120 сом × ~25 + сборка ~400)
    price: 3400,
    rating: 4.9,
    reviewCount: 16,
    occasions: ['birthday', 'other'],
    flowerTypes: ['orchid'],
    colors: ['pink', 'purple', 'orange'],
    isNew: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 2250 },
      { id: 'm', label: 'M', price: 3400 },
      { id: 'l', label: 'L', price: 4800 },
    ],
  },
  {
    id: '24',
    slug: 'sadovye-gortenzii',
    name: 'Садовые гортензии',
    composition: '5 гортензий разных цветов, флористическая зелень',
    images: [IMG.hydrangea[0], IMG.hydrangea[1]],
    // #24 — 5 гортензий: ~100–120 сом/голова × 5 + сборка 150 = 650–750 сом ✅ рынок
    // Оставляем 900 сом — включает упаковку и микс цветов. Конкурентная цена.
    price: 900,
    oldPrice: 1300,
    discount: 31,
    rating: 4.8,
    reviewCount: 29,
    occasions: ['birthday', 'wedding', 'mom'],
    flowerTypes: ['orchid'],
    colors: ['pink', 'blue', 'white'],
    isHit: true,
    inStock: true,
    deliverToday: false,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['basket', 'no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 590 },
      { id: 'm', label: 'M', price: 900, oldPrice: 1300 },
      { id: 'l', label: 'L', price: 1280 },
    ],
  },

  // ── Траурные ──────────────────────────────────────────────────────────────
  {
    id: '25',
    slug: 'traurnyy-buket',
    name: 'Траурный букет',
    composition: '15 белых хризантем, белые лилии, зелень — сдержанная элегантность',
    images: [IMG.funeral[0], IMG.funeral[1]],
    // #25 — Траурный букет: ~15 сом × 15 хризантем + лилии ~80 + зелень + сборка 100 ≈ 405 сом
    // Ставим 450 сом — достойная и доступная цена для данного случая.
    price: 450,
    rating: 4.8,
    reviewCount: 12,
    occasions: ['funeral'],
    flowerTypes: ['mixed'],
    colors: ['white'],
    inStock: true,
    deliverToday: true,
    kind: 'bouquet',
    freshness: 'live',
    packagingOptions: ['no_basket'],
    sizes: [
      { id: 's', label: 'S', price: 290 },
      { id: 'm', label: 'M', price: 450 },
      { id: 'l', label: 'L', price: 660 },
    ],
  },
]
