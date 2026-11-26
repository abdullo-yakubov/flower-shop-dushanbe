import { apiClient } from './client'

export type OrderStatus =
  | 'accepted'
  | 'assembling'
  | 'photo_ready'
  | 'courier_en_route'
  | 'delivered'
  | 'cancelled'

export interface ApiOrderSummary {
  id: string
  orderNumber: string
  status: OrderStatus
  total: number
  deliveryDate: string
  deliveryTime: string
  itemCount: number
  firstItemImage: string | null
  isCancellable: boolean
  createdAt: string
}

export interface ApiOrderStatusHistoryEntry {
  status: OrderStatus
  fromStatus: OrderStatus | null
  note: string | null
  photoUrl: string | null
  createdAt: string
}

export interface ApiOrderDetail extends ApiOrderSummary {
  statusHistory: ApiOrderStatusHistoryEntry[]
}

export interface CreateOrderPayload {
  items: {
    productId: string
    sizeId: string
    quantity: number
    packaging: 'basket' | 'no_basket'
    stemCount?: number
  }[]
  addons?: {
    addonId: string
    quantity: number
    inscription?: string
  }[]
  postcardId?: string
  cardText?: string
  address: string
  deliveryZoneId?: string
  deliveryDate: string
  deliveryTime: string
  paymentMethod: 'dc' | 'alif' | 'cash'
  customerName: string
  customerPhone: string
  isGift?: boolean
  recipientName?: string
  recipientPhone?: string
  isAnonymous?: boolean
  promoCode?: string
  redeemPoints?: number
}

export interface CreateOrderResult {
  orderId: string
  orderNumber: string
  total: number
  earnedPoints: number
  status: OrderStatus
}

export async function apiCreateOrder(payload: CreateOrderPayload): Promise<CreateOrderResult> {
  const { data } = await apiClient.post<{ success: true; data: CreateOrderResult }>(
    '/orders',
    payload
  )
  return data.data
}

export async function apiGetOrders(page = 1): Promise<{
  items: ApiOrderSummary[]
  total: number
}> {
  const { data } = await apiClient.get<{
    success: true
    data: ApiOrderSummary[]
    pagination: { total: number }
  }>('/orders', { params: { page, pageSize: 20 } })
  return { items: data.data, total: data.pagination.total }
}

export async function apiGetOrder(id: string): Promise<ApiOrderDetail> {
  const { data } = await apiClient.get<{ success: true; data: ApiOrderDetail }>(`/orders/${id}`)
  return data.data
}

export async function apiCancelOrder(id: string, reason?: string): Promise<void> {
  await apiClient.post(`/orders/${id}/cancel`, { reason: reason ?? '' })
}
