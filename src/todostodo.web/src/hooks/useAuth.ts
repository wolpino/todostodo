import { queryOptions, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { QueryClient } from '@tanstack/react-query'
import { getManageInfo, postLogin, postRegister } from '@/api/generated'
import type { InfoResponse, LoginRequest, RegisterRequest } from '@/api/generated'
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
 * Hit /manage/info and write the result into the auth cache.
 * Clears any cached pre-login null first — with staleTime: Infinity,
 * fetchQuery would otherwise return that stale value without refetching.
 */
export const syncAuthSession = async (queryClient: QueryClient): Promise<InfoResponse> => {
  queryClient.removeQueries({ queryKey: AUTH_QUERY_KEY })
  const { data, response } = await getManageInfo()
  if (!response?.ok || !data) {
    throw new HttpError(500, 'Sign in failed. Please try again.')
  }
  queryClient.setQueryData(AUTH_QUERY_KEY, data)
  return data
}

/**
 * Shared query options — used by both `useAuth()` and TanStack Router
 * route loaders (`context.queryClient.ensureQueryData(authQueryOptions)`).
 * `staleTime: Infinity` — auth only updates when login/logout mutations
 * explicitly update the cache.
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
      clearUserScopedQueries(queryClient)
      await syncAuthSession(queryClient)
    },
  })
}

export const useRegister = () => {
  return useMutation({
    mutationFn: async (body: RegisterRequest) => {
      const { error, response } = await postRegister({ body })
      if (!response?.ok) {
        throw new HttpError(response?.status ?? 400, getRegisterErrorMessage(error))
      }
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
