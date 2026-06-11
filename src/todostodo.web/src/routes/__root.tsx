import { createRootRouteWithContext, Outlet } from '@tanstack/react-router'
import { Box, Text } from '@chakra-ui/react'
import type { QueryClient } from '@tanstack/react-query'

export type RouterContext = {
  queryClient: QueryClient
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: () => <Outlet />,
  notFoundComponent: () => (
    <Box p={8} textAlign="center">
      <Text color="gray.500">404 — Page not found</Text>
    </Box>
  ),
})
