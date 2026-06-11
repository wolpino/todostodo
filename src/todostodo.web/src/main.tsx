import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ChakraProvider, defaultSystem } from '@chakra-ui/react'
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { createRouter, RouterProvider } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/router-devtools'
import './index.css'
import { routeTree } from './routeTree.gen'
import { client } from './api/generated/client.gen'
import { queryClient } from './lib/queryClient'

// Route all API calls through the Vite dev proxy (/api/* → localhost:5162)
client.setConfig({ baseUrl: '' })

const router = createRouter({
  routeTree,
  context: { queryClient },
})

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ChakraProvider value={defaultSystem}>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
        <ReactQueryDevtools initialIsOpen={false} />
        {import.meta.env.DEV && <TanStackRouterDevtools router={router} />}
      </QueryClientProvider>
    </ChakraProvider>
  </StrictMode>,
)
