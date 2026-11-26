import { apiClient } from './client'

export interface ApiAddress {
  id: string
  label: string
  fullAddress: string
  isDefault: boolean
  createdAt: string
}

export async function apiGetAddresses(): Promise<ApiAddress[]> {
  const { data } = await apiClient.get<{ success: true; data: ApiAddress[] }>('/addresses')
  return data.data
}

export async function apiCreateAddress(payload: {
  label: string
  fullAddress: string
}): Promise<ApiAddress> {
  const { data } = await apiClient.post<{ success: true; data: ApiAddress }>('/addresses', payload)
  return data.data
}

export async function apiUpdateAddress(
  id: string,
  payload: { label?: string; fullAddress?: string }
): Promise<ApiAddress> {
  const { data } = await apiClient.put<{ success: true; data: ApiAddress }>(
    `/addresses/${id}`,
    payload
  )
  return data.data
}

export async function apiDeleteAddress(id: string): Promise<void> {
  await apiClient.delete(`/addresses/${id}`)
}

export async function apiSetDefaultAddress(id: string): Promise<void> {
  await apiClient.patch(`/addresses/${id}/default`)
}
