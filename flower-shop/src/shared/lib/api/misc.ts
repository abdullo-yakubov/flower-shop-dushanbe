import { apiClient } from './client'
import type { Addon, Postcard, MessageTemplate } from '@/entities/product/types'

// ── Addons ────────────────────────────────────────────────────────────────────

interface ApiAddon {
  id: string
  type: string
  name: string
  imageUrl: string | null
  price: number
  hasInscription: boolean
}

export async function apiGetAddons(): Promise<Addon[]> {
  const { data } = await apiClient.get<{ success: true; data: ApiAddon[] }>('/addons')
  return data.data.map((a) => ({
    id: a.id,
    type: a.type as Addon['type'],
    name: a.name,
    image: a.imageUrl ?? '',
    price: a.price,
    hasInscription: a.hasInscription,
  }))
}

// ── Postcards ─────────────────────────────────────────────────────────────────

interface ApiPostcard {
  id: string
  occasion: string
  imageUrl: string | null
  emoji: string
  nameRu: string
  isPopular: boolean
}

export async function apiGetPostcards(occasion?: string): Promise<Postcard[]> {
  const params = occasion && occasion !== 'all' ? { occasion } : {}
  const { data } = await apiClient.get<{ success: true; data: ApiPostcard[] }>('/postcards', {
    params,
  })
  return data.data.map((p) => ({
    id: p.id,
    occasion: p.occasion as Postcard['occasion'],
    image: p.imageUrl ?? p.emoji,
    isPopular: p.isPopular,
  }))
}

// ── Message templates ─────────────────────────────────────────────────────────

interface ApiTemplate {
  id: string
  category: string
  textRu: string
  isPopular: boolean
}

export async function apiGetMessageTemplates(category?: string): Promise<MessageTemplate[]> {
  const params = category && category !== 'all' ? { category } : {}
  const { data } = await apiClient.get<{ success: true; data: ApiTemplate[] }>(
    '/message-templates',
    { params }
  )
  return data.data.map((t) => ({
    id: t.id,
    category: t.category as MessageTemplate['category'],
    text: t.textRu,
    isPopular: t.isPopular,
  }))
}

// ── Delivery zones ────────────────────────────────────────────────────────────

export interface DeliveryZone {
  id: string
  nameRu: string
  fee: number
  minFreeDelivery: number
  minHours: number
  maxHours: number
}

export async function apiGetDeliveryZones(): Promise<DeliveryZone[]> {
  const { data } = await apiClient.get<{ success: true; data: DeliveryZone[] }>('/delivery-zones')
  return data.data
}

// ── Promo code ────────────────────────────────────────────────────────────────

export interface PromoResult {
  code: string
  discountPercent: number
  discountFixed: number
  discountAmount: number
  label: string
}

export async function apiValidatePromo(code: string, orderTotal: number): Promise<PromoResult> {
  const { data } = await apiClient.post<{ success: true; data: PromoResult }>(
    '/promo-codes/validate',
    { code, orderTotal }
  )
  return data.data
}

// ── Shop config ───────────────────────────────────────────────────────────────

export interface ShopConfig {
  shopName: string
  shopPhone: string
  shopTelegram: string
  shopInstagram: string
  openTime: string
  closeTime: string
  defaultDeliveryFee: number
  freeDeliveryThreshold: number
  loyaltyEarnDivisor: number
  loyaltyMinRedeem: number
  loyaltyMaxRedeemPct: number
  holidayPricing: { month: number; day: number; nameRu: string; markupPercent: number }[]
  timeSlots: string[]
}

export async function apiGetConfig(): Promise<ShopConfig> {
  const { data } = await apiClient.get<{ success: true; data: ShopConfig }>('/config')
  return data.data
}
