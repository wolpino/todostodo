import { useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { setupCrossTabSync } from '@/lib/crossTabSync'

export function CrossTabSync() {
  const queryClient = useQueryClient()

  useEffect(() => setupCrossTabSync(queryClient), [queryClient])

  return null
}
