import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getManageInfo, postLogin, postRegister } from '@/api/generated'
import type { LoginRequest, RegisterRequest } from '@/api/generated'

export const AUTH_QUERY_KEY = ['auth'] as const

/**
 * Returns the current user info from the server.
 * `staleTime: Infinity` — auth is never refetched in the background;
 * it updates only when login/logout mutations explicitly invalidate the key.
 */
export const useAuth = () =>
  useQuery({
    queryKey: AUTH_QUERY_KEY,
    queryFn: async () => {
      const { data, response } = await getManageInfo()
      if (!response.ok) return null
      return data ?? null
    },
    staleTime: Infinity,
    retry: false,
  })

export const useLogin = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (body: LoginRequest) => {
      const { response } = await postLogin({ query: { useCookies: true }, body })
      if (!response.ok) throw new Error('Login failed')
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY }),
  })
}

export const useRegister = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (body: RegisterRequest) => {
      const { response } = await postRegister({ body })
      if (!response.ok) throw new Error('Registration failed')
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY }),
  })
}

export const useLogout = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const response = await fetch('/logout', { method: 'POST' })
      if (!response.ok) throw new Error('Logout failed')
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY }),
  })
}
