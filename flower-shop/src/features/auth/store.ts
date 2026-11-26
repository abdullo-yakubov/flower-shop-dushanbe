import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { setTokens, clearTokens } from '@/shared/lib/api/client'

export interface User {
  id: string
  name: string
  phone: string
  avatarUrl?: string
  role?: 'customer' | 'admin' | 'superadmin'
}

interface AuthStore {
  user: User | null
  isModalOpen: boolean
  login: (user: User, access: string, refresh: string) => void
  logout: () => void
  updateUser: (patch: Partial<Pick<User, 'name' | 'avatarUrl'>>) => void
  openModal: () => void
  closeModal: () => void
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      user: null,
      isModalOpen: false,

      login: (user, access, refresh) => {
        setTokens(access, refresh)
        set({ user, isModalOpen: false })
      },

      logout: () => {
        clearTokens()
        set({ user: null })
      },

      updateUser: (patch) => set((s) => ({ user: s.user ? { ...s.user, ...patch } : null })),

      openModal: () => set({ isModalOpen: true }),
      closeModal: () => set({ isModalOpen: false }),
    }),
    { name: 'flower-auth', partialize: (s) => ({ user: s.user }) }
  )
)

// Listen for token expiry from the axios interceptor
if (typeof window !== 'undefined') {
  window.addEventListener('flower:session-expired', () => {
    useAuthStore.getState().logout()
  })
}
