import { apiClient } from './client'

export interface LoyaltyTx {
  id: string
  type: 'earn' | 'redeem' | 'admin_adjust'
  amount: number
  balanceAfter: number
  description: string
  createdAt: string
}

export interface LoyaltyData {
  points: number
  totalEarned: number
  tier: 'bronze' | 'silver' | 'gold'
  tierLabel: string
  nextTierPoints: number | null
  pointsToNextTier: number | null
  history: LoyaltyTx[]
}

export async function apiGetLoyalty(): Promise<LoyaltyData> {
  const { data } = await apiClient.get<{ success: true; data: LoyaltyData }>('/loyalty')
  return data.data
}
