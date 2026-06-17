import { createRootRouteWithContext, Outlet } from '@tanstack/react-router'
import { Box, Text } from '@chakra-ui/react'
import type { QueryClient } from '@tanstack/react-query'
import { CrossTabSync } from '@/components/layout/CrossTabSync'

export type RouterContext = {
  queryClient: QueryClient
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: () => (
    <>
      {/* Sync React Query cache across tabs / pop-out windows — see lib/crossTabSync.ts */}
      <CrossTabSync />
      <Outlet />
    </>
  ),
  notFoundComponent: () => (
    <Box p={8} textAlign="center">
      <Text color="gray.500">404 — Page not found</Text>
    </Box>
  ),
})
