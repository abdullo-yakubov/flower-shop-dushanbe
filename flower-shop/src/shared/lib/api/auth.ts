import { apiClient } from './client'

export interface AuthUser {
  id: string
  name: string
  phone: string
  avatarUrl: string | null
  role: 'customer' | 'admin' | 'superadmin'
}

export interface SendOtpResult {
  expiresInSeconds: number
  maskedPhone: string
  /** Only present in development environment */
  devCode?: string
}

export interface VerifyOtpResult {
  accessToken: string
  refreshToken: string
  expiresIn: number
  user: AuthUser
}

export async function sendOtp(phone: string): Promise<SendOtpResult> {
  const { data } = await apiClient.post<{ success: true; data: SendOtpResult }>('/auth/send-otp', {
    phone,
  })
  return data.data
}

export async function verifyOtp(phone: string, code: string): Promise<VerifyOtpResult> {
  const { data } = await apiClient.post<{ success: true; data: VerifyOtpResult }>(
    '/auth/verify-otp',
    { phone, code }
  )
  return data.data
}

export async function refreshAuth(refreshToken: string): Promise<VerifyOtpResult> {
  const { data } = await apiClient.post<{ success: true; data: VerifyOtpResult }>('/auth/refresh', {
    refreshToken,
  })
  return data.data
}

export async function logout(): Promise<void> {
  await apiClient.post('/auth/logout')
}

export interface ProfileData {
  id: string
  name: string
  phone: string
  avatarUrl: string | null
  totalOrders: number
  totalSpent: number
  createdAt: string
}

export async function getProfile(): Promise<ProfileData> {
  const { data } = await apiClient.get<{ success: true; data: ProfileData }>('/account/profile')
  return data.data
}

export async function updateProfile(name: string): Promise<ProfileData> {
  const { data } = await apiClient.put<{ success: true; data: ProfileData }>('/account/profile', {
    name,
  })
  return data.data
}

export async function uploadAvatar(file: File): Promise<string> {
  const form = new FormData()
  form.append('avatar', file)
  const { data } = await apiClient.post<{ success: true; data: { avatarUrl: string } }>(
    '/account/avatar',
    form,
    { headers: { 'Content-Type': 'multipart/form-data' } }
  )
  return data.data.avatarUrl
}
