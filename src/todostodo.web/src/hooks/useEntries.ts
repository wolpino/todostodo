import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  deleteApiEntryById,
  getApiEntry,
  postApiEntry,
  putApiEntryById,
} from '@/api/generated'
import type { CreateEntryRequest, UpdateEntryRequest } from '@/api/generated'
import { toAnyEntry } from '@/types/entries'
import type { AnyEntry } from '@/types/entries'

export const ENTRIES_QUERY_KEY = ['entries'] as const

export const useEntries = () =>
  useQuery({
    queryKey: ENTRIES_QUERY_KEY,
    queryFn: async () => {
      const { data, response } = await getApiEntry()
      if (!response.ok) throw new Error('Failed to fetch entries')
      return (data ?? []).map(toAnyEntry)
    },
  })

// ---------------------------------------------------------------------------
// Create
// ---------------------------------------------------------------------------

export const useCreateEntry = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (body: CreateEntryRequest) => {
      const { data, response } = await postApiEntry({ body })
      if (!response.ok) throw new Error('Failed to create entry')
      return toAnyEntry(data!)
    },
    onSuccess: (newEntry) => {
      queryClient.setQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY, (old = []) => [
        ...old,
        newEntry,
      ])
    },
  })
}

// ---------------------------------------------------------------------------
// Update
// ---------------------------------------------------------------------------

export const useUpdateEntry = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...body }: UpdateEntryRequest & { id: number }) => {
      const { response } = await putApiEntryById({ path: { id }, body })
      if (!response.ok) throw new Error('Failed to update entry')
    },
    onMutate: async ({ id, ...updates }) => {
      await queryClient.cancelQueries({ queryKey: ENTRIES_QUERY_KEY })
      const previous = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)
      queryClient.setQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY, (old = []) =>
        old.map((e) => (e.id === id ? { ...e, ...updates } : e)),
      )
      return { previous }
    },
    onError: (_err, _vars, ctx) => {
      if (ctx?.previous) {
        queryClient.setQueryData(ENTRIES_QUERY_KEY, ctx.previous)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ENTRIES_QUERY_KEY })
    },
  })
}

// ---------------------------------------------------------------------------
// Delete
// ---------------------------------------------------------------------------

export const useDeleteEntry = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => {
      const { response } = await deleteApiEntryById({ path: { id } })
      if (!response.ok) throw new Error('Failed to delete entry')
    },
    onMutate: async (id) => {
      await queryClient.cancelQueries({ queryKey: ENTRIES_QUERY_KEY })
      const previous = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)
      queryClient.setQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY, (old = []) =>
        old.filter((e) => e.id !== id),
      )
      return { previous }
    },
    onError: (_err, _vars, ctx) => {
      if (ctx?.previous) {
        queryClient.setQueryData(ENTRIES_QUERY_KEY, ctx.previous)
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ENTRIES_QUERY_KEY })
    },
  })
}
