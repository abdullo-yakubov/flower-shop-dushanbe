import { apiClient } from './client'
import type { Product, ProductFilters, Review } from '@/entities/product/types'

interface ApiProductCard {
  id: string
  slug: string
  name: string
  composition: string
  images: string[]
  price: number
  oldPrice: number | null
  discount: number | null
  rating: number
  reviewCount: number
  occasions: string[]
  flowerTypes: string[]
  colors: string[]
  isNew: boolean
  isHit: boolean
  inStock: boolean
  deliverToday: boolean
  kind: string
  freshness: string
  packagingOptions: string[]
  sizes: { id: string; label: string; price: number; oldPrice: number | null }[]
  stemPrice: number | null
  stemOptions: number[]
}

function mapProduct(p: ApiProductCard): Product {
  return {
    id: p.id,
    slug: p.slug,
    name: p.name,
    composition: p.composition,
    images: p.images,
    price: p.price,
    oldPrice: p.oldPrice ?? undefined,
    discount: p.discount ?? undefined,
    rating: p.rating,
    reviewCount: p.reviewCount,
    occasions: p.occasions as Product['occasions'],
    flowerTypes: p.flowerTypes as Product['flowerTypes'],
    colors: p.colors,
    isNew: p.isNew,
    isHit: p.isHit,
    inStock: p.inStock,
    deliverToday: p.deliverToday,
    kind: p.kind as Product['kind'],
    freshness: p.freshness as Product['freshness'],
    packagingOptions: p.packagingOptions as Product['packagingOptions'],
    sizes: p.sizes.map((s) => ({
      id: s.id,
      label: s.label as 'S' | 'M' | 'L',
      price: s.price,
      oldPrice: s.oldPrice ?? undefined,
    })),
    stemPrice: p.stemPrice ?? undefined,
    stemOptions: p.stemOptions,
  }
}

interface PaginatedResponse<T> {
  success: true
  data: T[]
  pagination: { page: number; pageSize: number; total: number; totalPages: number }
}

export async function apiFetchProducts(
  filters: ProductFilters & { lang?: string }
): Promise<{ items: Product[]; total: number }> {
  const params: Record<string, string | number | boolean | undefined> = {
    page: filters.page ?? 1,
    pageSize: 12,
    lang: filters.lang ?? 'ru',
  }
  if (filters.q) params.q = filters.q
  if (filters.occasion) params.occasion = filters.occasion
  if (filters.deliverToday) params.deliverToday = true
  if (filters.priceMin != null) params.priceMin = filters.priceMin
  if (filters.priceMax != null) params.priceMax = filters.priceMax
  if (filters.freshness) params.freshness = filters.freshness
  if (filters.kind) params.kind = filters.kind
  if (filters.packaging) params.packaging = filters.packaging
  if (filters.sort) params.sort = filters.sort
  if (filters.flowerTypes?.length) params.flowerTypes = filters.flowerTypes.join(',')
  if (filters.colors?.length) params.colors = filters.colors.join(',')

  const { data } = await apiClient.get<PaginatedResponse<ApiProductCard>>('/products', { params })
  return { items: data.data.map(mapProduct), total: data.pagination.total }
}

export async function apiFetchProduct(slug: string): Promise<Product | null> {
  try {
    const { data } = await apiClient.get<{ success: true; data: ApiProductCard }>(
      `/products/${slug}`
    )
    return mapProduct(data.data)
  } catch (err: unknown) {
    if ((err as { response?: { status?: number } })?.response?.status === 404) return null
    throw err
  }
}

interface ApiReview {
  id: string
  authorName: string
  authorAvatar: string | null
  rating: number
  text: string
  photos: string[]
  helpfulCount: number
  adminReply: string | null
  date: string
}

export async function apiFetchReviews(slug: string): Promise<Review[]> {
  const { data } = await apiClient.get<PaginatedResponse<ApiReview>>(`/products/${slug}/reviews`, {
    params: { pageSize: 20 },
  })
  return data.data.map((r) => ({
    id: r.id,
    authorName: r.authorName,
    authorAvatar: r.authorAvatar ?? undefined,
    rating: r.rating,
    text: r.text,
    photos: r.photos.length ? r.photos : undefined,
    date: r.date,
    helpful: r.helpfulCount,
  }))
}

export async function apiSubmitReview(
  slug: string,
  payload: { rating: number; text: string; photos?: string[] }
): Promise<void> {
  await apiClient.post(`/products/${slug}/reviews`, payload)
}
