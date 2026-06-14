import { queryOptions, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { QueryClient } from '@tanstack/react-query'
import { getManageInfo, postLogin, postRegister } from '@/api/generated'
import type { LoginRequest, RegisterRequest } from '@/api/generated'
import { ENTRIES_QUERY_KEY } from '@/hooks/useEntries'
import { SETTINGS_QUERY_KEY } from '@/hooks/useSettings'
import { getLoginErrorMessage, getRegisterErrorMessage } from '@/lib/authErrors'
import { HttpError } from '@/lib/httpError'

export const AUTH_QUERY_KEY = ['auth'] as const

/** Entries and settings are per-user — drop them whenever auth identity changes. */
const clearUserScopedQueries = (queryClient: QueryClient) => {
  queryClient.removeQueries({ queryKey: ENTRIES_QUERY_KEY })
  queryClient.removeQueries({ queryKey: SETTINGS_QUERY_KEY })
}

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
      if (!response?.ok) {
        throw new HttpError(response?.status ?? 500, getLoginErrorMessage(response?.status ?? 500))
      }
    },
    onSuccess: () => {
      clearUserScopedQueries(queryClient)
      queryClient.refetchQueries({ queryKey: AUTH_QUERY_KEY })
    },
  })
}

export const useRegister = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (body: RegisterRequest) => {
      const { error, response } = await postRegister({ body })
      if (!response?.ok) {
        throw new HttpError(response?.status ?? 400, getRegisterErrorMessage(error))
      }
    },
    onSuccess: () => {
      clearUserScopedQueries(queryClient)
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY })
    },
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
    onSuccess: () => {
      queryClient.setQueryData(AUTH_QUERY_KEY, null)
      clearUserScopedQueries(queryClient)
    },
  })
}
