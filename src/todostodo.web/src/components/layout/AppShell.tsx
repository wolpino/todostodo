import type { ReactNode } from 'react'
import { Box } from '@chakra-ui/react'
import { APP_GREEN } from '@/lib/appTheme'

type AppShellProps = {
  children: ReactNode
  fontFamily?: string
  /** Notebook frame; disable on auth pages if preferred. */
  bordered?: boolean
}

/** Fixed-width notebook frame — grows with content, scrolls when list is long. */
export function AppShell({ children, fontFamily, bordered = true }: AppShellProps) {
  const isPopup = typeof window !== 'undefined' && window.opener != null

  return (
    <Box
      w="var(--app-width)"
      maxW="100vw"
      mx="auto"
      my="10px"
      maxH={isPopup ? 'calc(100dvh - 20px)' : 'calc(100dvh - 20px)'}
      h={isPopup ? 'calc(100dvh - 20px)' : 'auto'}
      display="flex"
      flexDirection="column"
      borderWidth={bordered ? '10px' : 0}
      borderColor={APP_GREEN}
      borderStyle="ridge"
      fontFamily={fontFamily}
      textAlign="left"
      boxSizing="border-box"
      overflow="hidden"
    >
      {children}
    </Box>
  )
}
