import { Award, Crown, Medal, type LucideIcon } from 'lucide-react'
import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { API_ENABLED } from '@/shared/lib/api/client'
import { apiGetLoyalty } from '@/shared/lib/api/loyalty'

export interface LoyaltyTx {
  id: string
  type: 'earn' | 'redeem'
  amount: number
  description: string
  date: string
}

export type LoyaltyTier = 'bronze' | 'silver' | 'gold'

export interface LoyaltyTierInfo {
  tier: LoyaltyTier
  label: string
  icon: LucideIcon
  minPoints: number
  nextTierPoints: number | null
  color: string
}

export const TIERS: LoyaltyTierInfo[] = [
  {
    tier: 'bronze',
    label: 'Бронза',
    icon: Medal,
    minPoints: 0,
    nextTierPoints: 300,
    color: 'text-orange-600',
  },
  {
    tier: 'silver',
    label: 'Серебро',
    icon: Award,
    minPoints: 300,
    nextTierPoints: 1000,
    color: 'text-slate-400',
  },
  {
    tier: 'gold',
    label: 'Золото',
    icon: Crown,
    minPoints: 1000,
    nextTierPoints: null,
    color: 'text-gold',
  },
]

export const EARN_DIVISOR = 20
export const REDEEM_RATE = 1
export const MIN_REDEEM = 50
export const MAX_REDEEM_PCT = 0.3

export function calcEarn(amount: number): number {
  return Math.floor(amount / EARN_DIVISOR)
}

export function getTier(totalEarned: number): LoyaltyTierInfo {
  return [...TIERS].reverse().find((t) => totalEarned >= t.minPoints) ?? TIERS[0]
}

interface LoyaltyStore {
  points: number
  totalEarned: number
  history: LoyaltyTx[]
  /** Optimistically add points (shown instantly, confirmed on next sync) */
  earnPoints: (amount: number, description?: string) => number
  redeemPoints: (pts: number) => boolean
  maxRedeemable: (orderTotal: number) => number
  /** Pull real balance from backend */
  syncFromServer: () => Promise<void>
  /** Reset to zero (on logout) */
  reset: () => void
}

export const useLoyaltyStore = create<LoyaltyStore>()(
  persist(
    (set, get) => ({
      points: 0,
      totalEarned: 0,
      history: [],

      earnPoints: (amount, description = 'За покупку') => {
        const pts = calcEarn(amount)
        if (pts <= 0) return 0
        const tx: LoyaltyTx = {
          id: `earn-${Date.now()}`,
          type: 'earn',
          amount: pts,
          description,
          date: new Date().toLocaleDateString('ru-RU', {
            day: 'numeric',
            month: 'short',
            year: 'numeric',
          }),
        }
        set((s) => ({
          points: s.points + pts,
          totalEarned: s.totalEarned + pts,
          history: [tx, ...s.history].slice(0, 50),
        }))
        return pts
      },

      redeemPoints: (pts) => {
        const { points } = get()
        if (pts < MIN_REDEEM || pts > points) return false
        const tx: LoyaltyTx = {
          id: `redeem-${Date.now()}`,
          type: 'redeem',
          amount: -pts,
          description: 'Списание за заказ',
          date: new Date().toLocaleDateString('ru-RU', {
            day: 'numeric',
            month: 'short',
            year: 'numeric',
          }),
        }
        set((s) => ({
          points: s.points - pts,
          history: [tx, ...s.history].slice(0, 50),
        }))
        return true
      },

      maxRedeemable: (orderTotal) => {
        const { points } = get()
        const cap = Math.floor(orderTotal * MAX_REDEEM_PCT)
        return Math.min(points, cap)
      },

      syncFromServer: async () => {
        if (!API_ENABLED) return
        try {
          const remote = await apiGetLoyalty()
          const history: LoyaltyTx[] = remote.history.map((tx) => ({
            id: tx.id,
            type: tx.type === 'admin_adjust' ? 'earn' : tx.type,
            amount: tx.amount,
            description: tx.description,
            date: new Date(tx.createdAt).toLocaleDateString('ru-RU', {
              day: 'numeric',
              month: 'short',
              year: 'numeric',
            }),
          }))
          set({ points: remote.points, totalEarned: remote.totalEarned, history })
        } catch {
          // silently ignore; cached data remains
        }
      },

      reset: () => set({ points: 0, totalEarned: 0, history: [] }),
    }),
    { name: 'flower-loyalty', version: 1, migrate: (s) => s }
  )
)
