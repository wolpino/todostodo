import { queryOptions, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getApiSettings, putApiSettings } from '@/api/generated'
import type { Settings, UpdateSettingsRequest } from '@/api/generated'
import { HttpError } from '@/lib/httpError'
import { broadcastSync } from '@/lib/crossTabSync'

export const SETTINGS_QUERY_KEY = ['settings'] as const

export const settingsQueryOptions = queryOptions({
  queryKey: SETTINGS_QUERY_KEY,
  queryFn: async () => {
    const { data, response } = await getApiSettings()
    if (!response?.ok) throw new HttpError(response?.status ?? 500, 'Failed to fetch settings')
    return data!
  },
  staleTime: 1000 * 60,
})

export const useSettings = () => useQuery(settingsQueryOptions)

export const useUpdateSettings = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (body: UpdateSettingsRequest) => {
      const { response } = await putApiSettings({ body })
      if (!response?.ok) throw new Error('Failed to update settings')
    },
    onMutate: async (body) => {
      await queryClient.cancelQueries({ queryKey: SETTINGS_QUERY_KEY })
      const previous = queryClient.getQueryData<Settings>(SETTINGS_QUERY_KEY)
      if (previous && body.font) {
        queryClient.setQueryData<Settings>(SETTINGS_QUERY_KEY, {
          ...previous,
          font: body.font,
        })
      }
      return { previous }
    },
    onError: (_err, _vars, ctx) => {
      if (ctx?.previous) {
        queryClient.setQueryData(SETTINGS_QUERY_KEY, ctx.previous)
      }
    },
    onSettled: () => {
      broadcastSync({ type: 'settings-changed' }) // tell other tabs/windows to refetch
    },
  })
}
