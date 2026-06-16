import { createFileRoute, redirect } from '@tanstack/react-router'
import { Box, Flex, Text } from '@chakra-ui/react'
import { authQueryOptions } from '@/hooks/useAuth'
import { settingsQueryOptions, useSettings } from '@/hooks/useSettings'
import { EntryList } from '@/components/features/EntryList'
import { AppShell } from '@/components/layout/AppShell'
import { FontApplier } from '@/components/layout/FontApplier'
import { OpenSmallWindowButton } from '@/components/layout/OpenSmallWindowButton'
import { SettingsMenu } from '@/components/layout/SettingsMenu'
import { fontStackFor } from '@/lib/fonts'

export const Route = createFileRoute('/')({
  beforeLoad: async ({ context }) => {
    const user = await context.queryClient.ensureQueryData(authQueryOptions)
    if (!user) throw redirect({ to: '/login' })
    await context.queryClient.ensureQueryData(settingsQueryOptions)
  },
  component: HomePage,
})

function HomePage() {
  const { data: settings } = useSettings()
  const fontFamily = fontStackFor(settings?.font)

  return (
    <AppShell fontFamily={fontFamily}>
      <FontApplier />
      <Flex as="header" align="center" py={4} px={4}>
        <Box w="32px" flexShrink={0}>
          <SettingsMenu />
        </Box>
        <Text flex={1} textAlign="center" fontWeight="600" letterSpacing="-0.02em">
          TodosToDo
        </Text>
        <Box w="32px" flexShrink={0} display="flex" justifyContent="flex-end">
          <OpenSmallWindowButton />
        </Box>
      </Flex>

      <Box as="main">
        <EntryList />
      </Box>
    </AppShell>
  )
}
