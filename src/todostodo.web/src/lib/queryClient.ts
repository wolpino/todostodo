import { QueryClient } from '@tanstack/react-query'
import { HttpError } from './httpError'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60,
      // Only retry transient failures. 4xx errors are deterministic — a second
      // attempt will produce the same result — so bail immediately. 5xx and
      // network errors (no status) are worth one retry.
      retry: (failureCount, error) => {
        if (error instanceof HttpError && error.status < 500) return false
        return failureCount < 1
      },
    },
  },
})
