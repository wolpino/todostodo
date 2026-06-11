import { act, renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { ReactNode } from 'react'

vi.mock('@/api/generated', () => ({
  getApiEntry: vi.fn(),
  postApiEntry: vi.fn(),
  putApiEntryById: vi.fn(),
  deleteApiEntryById: vi.fn(),
}))

import {
  deleteApiEntryById,
  getApiEntry,
  postApiEntry,
  putApiEntryById,
} from '@/api/generated'
import {
  ENTRIES_QUERY_KEY,
  useCreateEntry,
  useDeleteEntry,
  useEntries,
  useUpdateEntry,
} from '@/hooks/useEntries'
import type { AnyEntry } from '@/types/entries'

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeEntry(id: number, title: string): AnyEntry {
  return { entryType: 'Todo', id, title, status: 'Active', user: {} }
}

function makeWrapper(queryClient: QueryClient) {
  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

function makeQueryClient() {
  return new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
}

// ---------------------------------------------------------------------------
// useEntries
// ---------------------------------------------------------------------------

describe('useEntries', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = makeQueryClient()
    vi.clearAllMocks()
  })

  it('maps raw API entries through toAnyEntry', async () => {
    vi.mocked(getApiEntry).mockResolvedValue({
      data: [{ id: 1, title: 'Hello', user: {}, status: 'Active' }],
      response: { ok: true } as Response,
    })

    const { result } = renderHook(() => useEntries(), { wrapper: makeWrapper(queryClient) })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.[0]).toMatchObject({ id: 1, title: 'Hello', entryType: 'Todo' })
  })

  it('surfaces an error on non-ok response', async () => {
    vi.mocked(getApiEntry).mockResolvedValue({
      data: undefined,
      response: { ok: false } as Response,
    })

    const { result } = renderHook(() => useEntries(), { wrapper: makeWrapper(queryClient) })

    await waitFor(() => expect(result.current.isError).toBe(true))
  })
})

// ---------------------------------------------------------------------------
// useCreateEntry
// ---------------------------------------------------------------------------

describe('useCreateEntry', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = makeQueryClient()
    vi.clearAllMocks()
  })

  it('appends the new entry to the cache on success', async () => {
    queryClient.setQueryData(ENTRIES_QUERY_KEY, [makeEntry(1, 'Existing')])

    vi.mocked(postApiEntry).mockResolvedValue({
      data: { id: 2, title: 'New', user: {}, status: 'Active' },
      response: { ok: true } as Response,
    })

    const { result } = renderHook(() => useCreateEntry(), { wrapper: makeWrapper(queryClient) })

    await act(async () => {
      await result.current.mutateAsync({ title: 'New', status: 'Active' })
    })

    const entries = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)
    expect(entries).toHaveLength(2)
    expect(entries?.[1]).toMatchObject({ id: 2, title: 'New', entryType: 'Todo' })
  })
})

// ---------------------------------------------------------------------------
// useUpdateEntry
// ---------------------------------------------------------------------------

describe('useUpdateEntry', () => {
  let queryClient: QueryClient
  const initial = [makeEntry(1, 'First'), makeEntry(2, 'Second')]

  beforeEach(() => {
    queryClient = makeQueryClient()
    vi.clearAllMocks()
  })

  it('optimistically updates the entry before the server responds', async () => {
    queryClient.setQueryData(ENTRIES_QUERY_KEY, initial)

    // Delay so we can observe the cache before the mutation settles
    vi.mocked(putApiEntryById).mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve({ response: { ok: true } as Response }), 50)),
    )

    const { result } = renderHook(() => useUpdateEntry(), { wrapper: makeWrapper(queryClient) })

    act(() => { result.current.mutate({ id: 1, title: 'Updated', status: 'Active' }) })

    await waitFor(() => {
      const entry = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)?.find((e) => e.id === 1)
      expect(entry?.title).toBe('Updated')
    })
  })

  it('rolls back the cache when the server returns an error', async () => {
    queryClient.setQueryData(ENTRIES_QUERY_KEY, initial)

    vi.mocked(putApiEntryById).mockResolvedValue({ response: { ok: false } as Response })

    const { result } = renderHook(() => useUpdateEntry(), { wrapper: makeWrapper(queryClient) })

    await act(async () => {
      await result.current.mutateAsync({ id: 1, title: 'Updated', status: 'Active' }).catch(() => {})
    })

    await waitFor(() => {
      const entry = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)?.find((e) => e.id === 1)
      expect(entry?.title).toBe('First')
    })
  })

  it('leaves other entries untouched during an update', async () => {
    queryClient.setQueryData(ENTRIES_QUERY_KEY, initial)

    vi.mocked(putApiEntryById).mockResolvedValue({ response: { ok: true } as Response })

    const { result } = renderHook(() => useUpdateEntry(), { wrapper: makeWrapper(queryClient) })

    await act(async () => {
      await result.current.mutateAsync({ id: 1, title: 'Updated', status: 'Active' })
    })

    await waitFor(() => {
      const entry2 = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)?.find((e) => e.id === 2)
      expect(entry2?.title).toBe('Second')
    })
  })
})

// ---------------------------------------------------------------------------
// useDeleteEntry
// ---------------------------------------------------------------------------

describe('useDeleteEntry', () => {
  let queryClient: QueryClient
  const initial = [makeEntry(1, 'First'), makeEntry(2, 'Second')]

  beforeEach(() => {
    queryClient = makeQueryClient()
    vi.clearAllMocks()
  })

  it('optimistically removes the entry before the server responds', async () => {
    queryClient.setQueryData(ENTRIES_QUERY_KEY, initial)

    vi.mocked(deleteApiEntryById).mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve({ response: { ok: true } as Response }), 50)),
    )

    const { result } = renderHook(() => useDeleteEntry(), { wrapper: makeWrapper(queryClient) })

    act(() => { result.current.mutate(1) })

    await waitFor(() => {
      const entries = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)
      expect(entries?.find((e) => e.id === 1)).toBeUndefined()
      expect(entries).toHaveLength(1)
    })
  })

  it('rolls back the cache when the server returns an error', async () => {
    queryClient.setQueryData(ENTRIES_QUERY_KEY, initial)

    vi.mocked(deleteApiEntryById).mockResolvedValue({ response: { ok: false } as Response })

    const { result } = renderHook(() => useDeleteEntry(), { wrapper: makeWrapper(queryClient) })

    await act(async () => {
      await result.current.mutateAsync(1).catch(() => {})
    })

    await waitFor(() => {
      const entries = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)
      expect(entries).toHaveLength(2)
      expect(entries?.find((e) => e.id === 1)?.title).toBe('First')
    })
  })
})
