import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { API_ENABLED } from '@/shared/lib/api/client'
import {
  apiGetAddresses,
  apiCreateAddress,
  apiUpdateAddress,
  apiDeleteAddress,
  apiSetDefaultAddress,
} from '@/shared/lib/api/addresses'

export interface SavedAddress {
  id: string
  label: string
  /** Combined full address string (used by API) */
  fullAddress?: string
  /** Legacy local fields */
  street?: string
  apartment?: string
  district?: string
  isDefault: boolean
}

interface AddressesStore {
  addresses: SavedAddress[]
  add: (data: { label: string; fullAddress: string }) => Promise<string>
  update: (id: string, data: { label?: string; fullAddress?: string }) => Promise<void>
  remove: (id: string) => Promise<void>
  setDefault: (id: string) => Promise<void>
  getDefault: () => SavedAddress | undefined
  syncFromServer: () => Promise<void>
  reset: () => void
}

export const useAddressesStore = create<AddressesStore>()(
  persist(
    (set, get) => ({
      addresses: [],

      add: async (data) => {
        if (API_ENABLED) {
          const created = await apiCreateAddress(data)
          const addr: SavedAddress = {
            id: created.id,
            label: created.label,
            fullAddress: created.fullAddress,
            isDefault: created.isDefault,
          }
          set((s) => ({
            addresses: [...s.addresses, addr],
          }))
          return created.id
        }

        const id = `addr-${Date.now()}`
        const isFirst = get().addresses.length === 0
        set((s) => ({
          addresses: [
            ...s.addresses,
            { id, label: data.label, fullAddress: data.fullAddress, isDefault: isFirst },
          ],
        }))
        return id
      },

      update: async (id, data) => {
        if (API_ENABLED) {
          await apiUpdateAddress(id, data)
        }
        set((s) => ({
          addresses: s.addresses.map((a) => (a.id === id ? { ...a, ...data } : a)),
        }))
      },

      remove: async (id) => {
        if (API_ENABLED) {
          await apiDeleteAddress(id)
        }
        set((s) => {
          const filtered = s.addresses.filter((a) => a.id !== id)
          const hasDefault = filtered.some((a) => a.isDefault)
          return {
            addresses: filtered.map((a, i) =>
              !hasDefault && i === 0 ? { ...a, isDefault: true } : a
            ),
          }
        })
      },

      setDefault: async (id) => {
        if (API_ENABLED) {
          await apiSetDefaultAddress(id)
        }
        set((s) => ({
          addresses: s.addresses.map((a) => ({ ...a, isDefault: a.id === id })),
        }))
      },

      getDefault: () => get().addresses.find((a) => a.isDefault),

      syncFromServer: async () => {
        if (!API_ENABLED) return
        try {
          const remote = await apiGetAddresses()
          set({
            addresses: remote.map((a) => ({
              id: a.id,
              label: a.label,
              fullAddress: a.fullAddress,
              isDefault: a.isDefault,
            })),
          })
        } catch {
          // silently ignore; cached data remains
        }
      },

      reset: () => set({ addresses: [] }),
    }),
    { name: 'flower-addresses', version: 2, migrate: (s) => s }
  )
)
