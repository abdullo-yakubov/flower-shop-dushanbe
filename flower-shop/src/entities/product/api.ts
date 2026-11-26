import type { Product, ProductFilters, Review } from './types'
import { MOCK_PRODUCTS } from './mock'
import { API_ENABLED } from '@/shared/lib/api/client'
import { apiFetchProducts, apiFetchProduct, apiFetchReviews } from '@/shared/lib/api/products'

export async function fetchProducts(
  filters: ProductFilters = {}
): Promise<{ items: Product[]; total: number }> {
  if (API_ENABLED) {
    return apiFetchProducts(filters)
  }

  // ── Mock fallback ──────────────────────────────────────────────────────────
  await new Promise((r) => setTimeout(r, 400))
  let items = [...MOCK_PRODUCTS]

  if (filters.q) {
    const q = filters.q.toLowerCase()
    items = items.filter(
      (p) => p.name.toLowerCase().includes(q) || p.composition.toLowerCase().includes(q)
    )
  }
  if (filters.occasion) items = items.filter((p) => p.occasions.includes(filters.occasion!))
  if (filters.deliverToday) items = items.filter((p) => p.deliverToday)
  if (filters.priceMin != null) items = items.filter((p) => p.price >= filters.priceMin!)
  if (filters.priceMax != null) items = items.filter((p) => p.price <= filters.priceMax!)
  if (filters.freshness) items = items.filter((p) => p.freshness === filters.freshness)
  if (filters.kind) items = items.filter((p) => p.kind === filters.kind)
  if (filters.packaging)
    items = items.filter((p) => p.packagingOptions.includes(filters.packaging!))

  if (filters.sort === 'price_asc') items.sort((a, b) => a.price - b.price)
  if (filters.sort === 'price_desc') items.sort((a, b) => b.price - a.price)
  if (filters.sort === 'rating') items.sort((a, b) => b.rating - a.rating)
  if (filters.sort === 'newest')
    items = items.filter((p) => p.isNew).concat(items.filter((p) => !p.isNew))

  const page = filters.page || 1
  const pageSize = 12
  return { items: items.slice((page - 1) * pageSize, page * pageSize), total: items.length }
}

export async function fetchProduct(slug: string): Promise<Product | null> {
  if (API_ENABLED) {
    return apiFetchProduct(slug)
  }
  await new Promise((r) => setTimeout(r, 200))
  return MOCK_PRODUCTS.find((p) => p.slug === slug) || null
}

export async function fetchReviews(slugOrId: string): Promise<Review[]> {
  if (API_ENABLED) {
    return apiFetchReviews(slugOrId)
  }
  await new Promise((r) => setTimeout(r, 300))
  return [
    {
      id: '1',
      authorName: 'Камилла Р.',
      rating: 5,
      text: 'Восхитительные цветы, свежие и ароматные! Доставили точно вовремя.',
      date: '2024-05-10',
      helpful: 12,
    },
    {
      id: '2',
      authorName: 'Нилуфар А.',
      rating: 4,
      text: 'Красивый букет, соответствует фото. Упаковка аккуратная.',
      date: '2024-04-28',
      helpful: 7,
      photos: ['https://images.unsplash.com/photo-1548142813-c348350df52b?w=300&h=300&fit=crop'],
    },
    {
      id: '3',
      authorName: 'Диловар Х.',
      rating: 5,
      text: 'Заказывал для мамы — она была в восторге! Стоят уже 10 дней.',
      date: '2024-04-15',
      helpful: 19,
    },
  ]
}
