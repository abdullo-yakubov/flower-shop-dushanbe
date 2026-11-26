import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { API_ENABLED } from '@/shared/lib/api/client'
import {
  apiGetOrders,
  apiGetOrder,
  apiCancelOrder,
  type ApiOrderSummary,
  type ApiOrderStatusHistoryEntry,
} from '@/shared/lib/api/orders'

export type OrderStatus =
  | 'Принят'
  | 'Собирается'
  | 'Фото готово'
  | 'Курьер в пути'
  | 'Доставлен'
  | 'Отменён'

const STATUS_MAP: Record<ApiOrderSummary['status'], OrderStatus> = {
  accepted: 'Принят',
  assembling: 'Собирается',
  photo_ready: 'Фото готово',
  courier_en_route: 'Курьер в пути',
  delivered: 'Доставлен',
  cancelled: 'Отменён',
}

export interface OrderItem {
  productId: string
  productName: string
  productImg: string
  size: string
  quantity: number
  price: number
}

export interface OrderAddon {
  id: string
  name: string
  price: number
  quantity: number
  inscription?: string
}

export interface OrderStatusHistoryEntry {
  status: OrderStatus
  fromStatus: OrderStatus | null
  note: string | null
  photoUrl: string | null
  createdAt: string
}

export interface Order {
  id: string
  /** Server-issued GUID (present once confirmed) — required for detail/cancel API calls, distinct from the human-readable `id`/order number shown in the UI */
  serverId?: string
  date: string
  items: OrderItem[]
  addons: OrderAddon[]
  cardText?: string
  subtotal: number
  deliveryFee: number
  promoDiscount: number
  pointsDiscount: number
  total: number
  status: OrderStatus
  address: string
  deliveryDate: string
  deliveryTime: string
  payment: 'dc' | 'alif' | 'cash'
  isCancellable: boolean
  statusHistory?: OrderStatusHistoryEntry[]
}

function mapHistory(entries: ApiOrderStatusHistoryEntry[]): OrderStatusHistoryEntry[] {
  return entries.map((h) => ({
    status: STATUS_MAP[h.status],
    fromStatus: h.fromStatus ? STATUS_MAP[h.fromStatus] : null,
    note: h.note,
    photoUrl: h.photoUrl,
    createdAt: h.createdAt,
  }))
}

interface OrdersStore {
  orders: Order[]
  /** Add a locally-created order (before API response arrives) */
  addOrder: (data: Omit<Order, 'id' | 'date' | 'status' | 'isCancellable'>) => string
  /** Override order id after server confirms it */
  confirmOrder: (tempId: string, serverId: string, serverNumber: string) => void
  /** Drop an optimistic order that the server rejected */
  discardOrder: (tempId: string) => void
  cancelOrder: (id: string, reason?: string) => Promise<void>
  /** Pull orders from backend and merge */
  syncFromServer: () => Promise<void>
  /** Fetch full status history + delivery photo for one order */
  fetchOrderDetail: (id: string) => Promise<void>
}

export const useOrdersStore = create<OrdersStore>()(
  persist(
    (set, get) => ({
      orders: [],

      addOrder: (data) => {
        const tempId = `#${Math.floor(100000 + Math.random() * 900000)}`
        set((s) => ({
          orders: [
            {
              ...data,
              id: tempId,
              date: new Date().toLocaleDateString('ru-RU', {
                day: 'numeric',
                month: 'long',
                year: 'numeric',
              }),
              status: 'Принят' as OrderStatus,
              isCancellable: true,
            },
            ...s.orders,
          ],
        }))
        return tempId
      },

      confirmOrder: (tempId, serverId, serverNumber) => {
        set((s) => ({
          orders: s.orders.map((o) =>
            o.id === tempId ? { ...o, id: serverNumber || serverId, serverId } : o
          ),
        }))
      },

      discardOrder: (tempId) => {
        set((s) => ({ orders: s.orders.filter((o) => o.id !== tempId) }))
      },

      cancelOrder: async (id, reason) => {
        const order = get().orders.find((o) => o.id === id)
        if (!order || order.status !== 'Принят') return

        if (API_ENABLED && order.serverId) {
          await apiCancelOrder(order.serverId, reason)
        }

        set((s) => ({
          orders: s.orders.map((o) =>
            o.id === id ? { ...o, status: 'Отменён' as OrderStatus, isCancellable: false } : o
          ),
        }))
      },

      fetchOrderDetail: async (id) => {
        if (!API_ENABLED) return
        const order = get().orders.find((o) => o.id === id)
        if (!order?.serverId) return
        try {
          const detail = await apiGetOrder(order.serverId)
          set((s) => ({
            orders: s.orders.map((o) =>
              o.id === id
                ? {
                    ...o,
                    status: STATUS_MAP[detail.status],
                    isCancellable: detail.isCancellable,
                    statusHistory: mapHistory(detail.statusHistory),
                  }
                : o
            ),
          }))
        } catch {
          // silently ignore; cached order remains
        }
      },

      syncFromServer: async () => {
        if (!API_ENABLED) return
        try {
          const { items } = await apiGetOrders()
          // Merge server data: update status on existing orders, add missing ones
          set((s) => {
            const localMap = new Map(s.orders.map((o) => [o.id, o]))
            for (const srv of items) {
              const key = srv.orderNumber
              if (localMap.has(key)) {
                const existing = localMap.get(key)!
                localMap.set(key, {
                  ...existing,
                  serverId: srv.id,
                  status: STATUS_MAP[srv.status],
                  isCancellable: srv.isCancellable,
                })
              }
            }
            return { orders: Array.from(localMap.values()) }
          })
        } catch {
          // silently ignore; cached orders remain
        }
      },
    }),
    {
      name: 'flower-orders',
      version: 2,
      migrate: (state, version) => {
        const s = state as Partial<OrdersStore> & { orders?: Partial<Order>[] }
        if (version < 1 || version < 2) {
          return {
            ...s,
            orders: (s.orders ?? []).map((o) => ({
              ...o,
              subtotal: o.subtotal ?? 0,
              deliveryFee: o.deliveryFee ?? 0,
              promoDiscount: o.promoDiscount ?? 0,
              pointsDiscount: o.pointsDiscount ?? 0,
            })),
          }
        }
        return s
      },
    }
  )
)
