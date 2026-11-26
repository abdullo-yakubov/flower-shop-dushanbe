import axios, { type AxiosError } from 'axios'

// Set VITE_API_URL in .env (e.g. http://localhost:5000)
// If empty, the app falls back to mock data throughout
const BASE = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? ''

export const API_ENABLED = Boolean(BASE)

export const apiClient = axios.create({
  baseURL: `${BASE}/api/v1`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000,
})

// Token storage
// Access token lives in sessionStorage (cleared on tab close, ≤ 15 min TTL anyway)
// Refresh token lives in localStorage (persists 30 days)

const KEYS = {
  access: 'flower_access',
  refresh: 'flower_refresh',
} as const

export function getAccessToken(): string | null {
  return sessionStorage.getItem(KEYS.access) ?? localStorage.getItem(KEYS.access)
}

export function setTokens(access: string, refresh: string): void {
  sessionStorage.setItem(KEYS.access, access)
  localStorage.setItem(KEYS.access, access) // also localStorage so it survives F5
  localStorage.setItem(KEYS.refresh, refresh)
}

export function clearTokens(): void {
  sessionStorage.removeItem(KEYS.access)
  localStorage.removeItem(KEYS.access)
  localStorage.removeItem(KEYS.refresh)
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(KEYS.refresh)
}

// Request interceptor: attach JWT
apiClient.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Response interceptor: auto-refresh on 401
let refreshPromise: Promise<string> | null = null

apiClient.interceptors.response.use(
  (res) => res,
  async (err: AxiosError) => {
    const original = err.config as typeof err.config & { _retry?: boolean }
    if (err.response?.status === 401 && original && !original._retry) {
      original._retry = true
      try {
        if (!refreshPromise) {
          refreshPromise = performRefresh().finally(() => {
            refreshPromise = null
          })
        }
        const newAccess = await refreshPromise
        original.headers = original.headers ?? {}
        original.headers.Authorization = `Bearer ${newAccess}`
        return apiClient(original)
      } catch {
        clearTokens()
        // Notify the app that the session ended
        window.dispatchEvent(new Event('flower:session-expired'))
      }
    }
    return Promise.reject(err)
  }
)

async function performRefresh(): Promise<string> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) throw new Error('no_refresh_token')

  const { data } = await axios.post<{
    success: boolean
    data: { accessToken: string; refreshToken: string }
  }>(`${BASE}/api/v1/auth/refresh`, { refreshToken })

  const { accessToken, refreshToken: newRefresh } = data.data
  setTokens(accessToken, newRefresh)
  return accessToken
}

// Typed error helper
export interface ApiError {
  code: string
  message: string
  details?: { field: string; message: string }[]
}

export function extractError(err: unknown): ApiError {
  if (axios.isAxiosError(err)) {
    const body = (err.response?.data as { error?: ApiError } | undefined)?.error
    if (body) return body
    return { code: 'NETWORK_ERROR', message: err.message }
  }
  return { code: 'UNKNOWN', message: String(err) }
}
