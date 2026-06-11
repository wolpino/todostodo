import { queryOptions, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getManageInfo, postLogin, postRegister } from '@/api/generated'
import type { LoginRequest, RegisterRequest } from '@/api/generated'

export const AUTH_QUERY_KEY = ['auth'] as const

/**
 * Shared query options — used by both `useAuth()` and TanStack Router
 * route loaders (`context.queryClient.ensureQueryData(authQueryOptions)`).
 * `staleTime: Infinity` — auth only updates when login/logout mutations
 * explicitly invalidate the key.
 */
export const authQueryOptions = queryOptions({
  queryKey: AUTH_QUERY_KEY,
  queryFn: async () => {
    const { data, response } = await getManageInfo()
    if (!response?.ok) return null
    return data ?? null
  },
  staleTime: Infinity,
  retry: false,
})

export const useAuth = () => useQuery(authQueryOptions)

export const useLogin = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (body: LoginRequest) => {
      const { response } = await postLogin({ query: { useCookies: true }, body })
      if (!response?.ok) throw new Error('Login failed')
    },
    onSuccess: () => queryClient.refetchQueries({ queryKey: AUTH_QUERY_KEY }),
  })
}

export const useRegister = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (body: RegisterRequest) => {
      const { response } = await postRegister({ body })
      if (!response?.ok) throw new Error('Registration failed')
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY }),
  })
}

export const useLogout = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const response = await fetch('/logout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({}),
      })
      if (!response.ok) throw new Error('Logout failed')
    },
    onSuccess: () => queryClient.setQueryData(AUTH_QUERY_KEY, null),
  })
}
