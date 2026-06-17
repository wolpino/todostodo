import type { QueryClient } from '@tanstack/react-query'
import { ENTRIES_QUERY_KEY } from '@/hooks/useEntries'
import { SETTINGS_QUERY_KEY } from '@/hooks/useSettings'

/**
 * Cross-tab / cross-window sync for the same logged-in user.
 *
 * Problem: opening the app in a pop-out window (or a second browser tab) gives each
 * window its own React Query cache. A status change in one window won't appear in
 * the other until a full page reload.
 *
 * Solution (two layers):
 * 1. BroadcastChannel — when one window mutates data, it posts a message; siblings
 *    invalidate their cache and refetch from the API.
 * 2. window "focus" — when the user switches back to a tab, refetch as a safety net
 *    (covers missed messages or browsers without BroadcastChannel).
 *
 * Note: both windows share the same auth cookie and hit the same API. The database
 * is already consistent; we only need to keep each window's in-memory cache fresh.
 */

/** All tabs/windows for this app listen on the same channel name. */
const CHANNEL_NAME = 'todostodo-sync'

export type SyncMessage =
  | { type: 'entries-changed' }
  | { type: 'settings-changed' }

/**
 * Notify other open tabs/windows that data changed in this one.
 * Call this from mutation `onSuccess` / `onSettled` after writes complete.
 */
export function broadcastSync(message: SyncMessage) {
  if (typeof BroadcastChannel === 'undefined') return
  const channel = new BroadcastChannel(CHANNEL_NAME)
  channel.postMessage(message)
  channel.close()
}

/**
 * Wire up listeners once per app load. Returns a cleanup function for React effects.
 */
export function setupCrossTabSync(queryClient: QueryClient) {
  if (typeof window === 'undefined') return () => {}

  // invalidateQueries marks cached data stale; active useQuery hooks refetch automatically.
  const invalidateEntries = () => {
    queryClient.invalidateQueries({ queryKey: ENTRIES_QUERY_KEY })
  }

  const invalidateSettings = () => {
    queryClient.invalidateQueries({ queryKey: SETTINGS_QUERY_KEY })
  }

  // Fallback: refetch when the user returns to this tab (e.g. after editing in pop-out).
  const onFocus = () => {
    invalidateEntries()
    invalidateSettings()
  }

  window.addEventListener('focus', onFocus)

  // Primary: hear broadcasts from sibling tabs/windows on the same origin.
  let channel: BroadcastChannel | undefined
  if (typeof BroadcastChannel !== 'undefined') {
    channel = new BroadcastChannel(CHANNEL_NAME)
    channel.onmessage = (event: MessageEvent<SyncMessage>) => {
      if (event.data?.type === 'entries-changed') invalidateEntries()
      if (event.data?.type === 'settings-changed') invalidateSettings()
    }
  }

  return () => {
    window.removeEventListener('focus', onFocus)
    channel?.close()
  }
}
