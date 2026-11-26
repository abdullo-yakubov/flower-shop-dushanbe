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

export const MAX_CART_LINE_QUANTITY = 20

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
  promoFixed: number
  discountAmount: () => number
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
      promoFixed: 0,

      addItem: (product, size, opts = {}) => {
        if (!product.inStock || !product.sizes.some((variant) => variant.id === size.id)) return
        const packaging = opts.packaging ?? (
          product.packagingOptions.includes('no_basket') ? 'no_basket' : product.packagingOptions[0] ?? 'no_basket'
        )
        if (product.packagingOptions.length && !product.packagingOptions.includes(packaging)) return
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
                i.id === key ? { ...i, quantity: Math.min(MAX_CART_LINE_QUANTITY, i.quantity + 1) } : i
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
        if (!Number.isInteger(quantity)) return
        if (quantity <= 0) {
          get().removeItem(id)
          return
        }
        set((state) => ({
          items: state.items.map((i) => i.id === id ? { ...i, quantity: Math.min(MAX_CART_LINE_QUANTITY, quantity) } : i),
        }))
      },

      clearCart: () => set({ items: [], promoCode: '', promoDiscount: 0, promoFixed: 0 }),
      openCart: () => set({ isOpen: true }),
      closeCart: () => set({ isOpen: false }),
      toggleCart: () => set((s) => ({ isOpen: !s.isOpen })),

      subtotal: () => get().items.reduce((s, i) => s + i.snapshotPrice * i.quantity, 0),

      discountAmount: () => {
        const { subtotal, promoDiscount, promoFixed, promoCode } = get()
        const amount = subtotal()
        if (!promoCode) return 0
        if (!API_ENABLED && amount < (findPromo(promoCode)?.minOrder ?? Infinity)) return 0
        return Math.min(amount, promoFixed || Math.round((amount * promoDiscount) / 100))
      },
      total: () => get().subtotal() - get().discountAmount(),

      count: () => get().items.reduce((s, i) => s + i.quantity, 0),

      applyPromo: async (code) => {
        const subtotal = get().subtotal()
        if (API_ENABLED) {
          try {
            const res = await apiValidatePromo(code.trim(), subtotal)
            if (res.discountAmount <= 0) return false
            set({ promoCode: res.code, promoDiscount: res.discountPercent, promoFixed: res.discountFixed })
            return true
          } catch {
            return false
          }
        }
        await new Promise((r) => setTimeout(r, 400))
        const promo = findPromo(code)
        if (!promo || subtotal < promo.minOrder) return false
        set({ promoCode: promo.code, promoDiscount: promo.discountPct, promoFixed: 0 })
        return true
      },

      clearPromo: () => set({ promoCode: '', promoDiscount: 0, promoFixed: 0 }),
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
