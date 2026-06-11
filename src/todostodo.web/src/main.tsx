import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ChakraProvider, createSystem, defaultConfig } from '@chakra-ui/react'
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

// set system font in Chakra
const systemComicShanns = createSystem(defaultConfig, {
  theme: {
    tokens: {
      fonts: {
        body: { value: "'Comic Shanns', system-ui, sans-serif" },
        heading: { value: "'Comic Shanns', system-ui, sans-serif" },
        mono: { value: "'Comic Shanns', ui-monospace, monospace" },
      },
    },
  },
})

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}


createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ChakraProvider value={systemComicShanns}>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
        <ReactQueryDevtools initialIsOpen={false} />
        {import.meta.env.DEV && <TanStackRouterDevtools router={router} />}
      </QueryClientProvider>
    </ChakraProvider>
  </StrictMode>,
)
