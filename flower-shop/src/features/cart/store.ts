import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { Product, ProductSize, Packaging } from '@/entities/product/types'
import { findPromo } from '@/shared/config/promotionConfig'
import { BASKET_SURCHARGE } from '@/shared/config/packagingConfig'
import { API_ENABLED } from '@/shared/lib/api/client'
import { apiValidatePromo } from '@/shared/lib/api/misc'

export interface CartItem {
  id: string
  product: Product
  size: ProductSize
  /** Unit price at the time the item was added (incl. basket surcharge). The server re-prices on checkout. */
  snapshotPrice: number
  quantity: number
  packaging: Packaging
  stemCount?: number
}

interface CartStore {
  items: CartItem[]
  isOpen: boolean
  addItem: (
    product: Product,
    size: ProductSize,
    opts?: { packaging?: Packaging; stemCount?: number; snapshotPrice?: number }
  ) => void
  removeItem: (id: string) => void
  updateQuantity: (id: string, quantity: number) => void
  clearCart: () => void
  openCart: () => void
  closeCart: () => void
  toggleCart: () => void
  /** Items subtotal before promo */
  subtotal: () => number
  /** Items subtotal after promo */
  total: () => number
  count: () => number
  promoCode: string
  promoDiscount: number
  applyPromo: (code: string) => Promise<boolean>
  clearPromo: () => void
}

/** Unit price for a cart line — must match PricingService on the backend (holiday markup aside). */
export function unitPrice(
  product: Product,
  size: ProductSize,
  packaging: Packaging,
  stemCount?: number
): number {
  const base = stemCount != null && product.stemPrice ? product.stemPrice * stemCount : size.price
  return base + (packaging === 'basket' ? BASKET_SURCHARGE : 0)
}

export const useCartStore = create<CartStore>()(
  persist(
    (set, get) => ({
      items: [],
      isOpen: false,
      promoCode: '',
      promoDiscount: 0,

      addItem: (product, size, opts = {}) => {
        const packaging = opts.packaging ?? 'no_basket'
        const stemCount = opts.stemCount
        const snapshotPrice = opts.snapshotPrice ?? unitPrice(product, size, packaging, stemCount)
        const key =
          stemCount != null
            ? `${product.id}-stem${stemCount}-${packaging}`
            : `${product.id}-${size.id}-${packaging}`

        set((state) => {
          const existing = state.items.find((i) => i.id === key)
          if (existing) {
            return {
              items: state.items.map((i) =>
                i.id === key ? { ...i, quantity: i.quantity + 1 } : i
              ),
            }
          }
          return {
            items: [
              ...state.items,
              { id: key, product, size, snapshotPrice, quantity: 1, packaging, stemCount },
            ],
          }
        })
      },

      removeItem: (id) => set((state) => ({ items: state.items.filter((i) => i.id !== id) })),

      updateQuantity: (id, quantity) => {
        if (quantity <= 0) {
          get().removeItem(id)
          return
        }
        set((state) => ({ items: state.items.map((i) => (i.id === id ? { ...i, quantity } : i)) }))
      },

      clearCart: () => set({ items: [], promoCode: '', promoDiscount: 0 }),
      openCart: () => set({ isOpen: true }),
      closeCart: () => set({ isOpen: false }),
      toggleCart: () => set((s) => ({ isOpen: !s.isOpen })),

      subtotal: () => get().items.reduce((s, i) => s + i.snapshotPrice * i.quantity, 0),

      total: () => {
        const subtotal = get().subtotal()
        const { promoDiscount } = get()
        return subtotal - Math.round((subtotal * promoDiscount) / 100)
      },

      count: () => get().items.reduce((s, i) => s + i.quantity, 0),

      applyPromo: async (code) => {
        const subtotal = get().subtotal()
        if (API_ENABLED) {
          try {
            const res = await apiValidatePromo(code.trim(), subtotal)
            if (res.discountPercent <= 0) return false
            set({ promoCode: res.code, promoDiscount: res.discountPercent })
            return true
          } catch {
            return false
          }
        }
        await new Promise((r) => setTimeout(r, 400))
        const promo = findPromo(code)
        if (!promo || subtotal < promo.minOrder) return false
        set({ promoCode: promo.code, promoDiscount: promo.discountPct })
        return true
      },

      clearPromo: () => set({ promoCode: '', promoDiscount: 0 }),
    }),
    {
      name: 'flower-cart',
      version: 2,
      // v0→v1: CartItem gained snapshotPrice. v1→v2: snapshotPrice now includes the basket surcharge.
      // Both cases are handled by recomputing the unit price from the stored product/size.
      migrate: (state, version) => {
        const s = state as CartStore
        if (version >= 2) return s
        return {
          ...s,
          items: (s.items ?? []).map((item) => ({
            ...item,
            snapshotPrice: item.size
              ? unitPrice(item.product, item.size, item.packaging ?? 'no_basket', item.stemCount)
              : (item.snapshotPrice ?? 0),
          })),
        }
      },
    }
  )
)
