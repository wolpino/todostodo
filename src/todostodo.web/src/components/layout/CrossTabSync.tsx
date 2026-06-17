import { useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { setupCrossTabSync } from '@/lib/crossTabSync'

/**
 * Invisible root component — registers cross-tab sync for the lifetime of the app.
 * Mounted once in __root.tsx so every route benefits without duplicating setup.
 */
export function CrossTabSync() {
  const queryClient = useQueryClient()

  useEffect(() => setupCrossTabSync(queryClient), [queryClient])

  return null
}
