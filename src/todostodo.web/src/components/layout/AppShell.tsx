import type { ReactNode } from 'react'
import { Box } from '@chakra-ui/react'

type AppShellProps = {
  children: ReactNode
  fontFamily?: string
}

/** Fixed-width notebook frame — sized for a narrow side-by-side window. */
export function AppShell({ children, fontFamily }: AppShellProps) {
  return (
    <Box
      w="var(--app-width)"
      maxW="100vw"
      mx="auto"
      minH="100svh"
      borderWidth="5px"
      borderColor="black"
      fontFamily={fontFamily}
      textAlign="left"
      boxSizing="border-box"
    >
      {children}
    </Box>
  )
}
