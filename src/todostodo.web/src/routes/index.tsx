import { createFileRoute, redirect } from '@tanstack/react-router'
import { Box, Flex, Text } from '@chakra-ui/react'
import { authQueryOptions } from '@/hooks/useAuth'
import { settingsQueryOptions, useSettings } from '@/hooks/useSettings'
import { EntryList } from '@/components/features/EntryList'
import { FontApplier } from '@/components/layout/FontApplier'
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
    <Box
      w="100%"
      maxW="500px"
      mx="auto"
      minH="100svh"
      borderWidth="5px"
      borderColor="black"
      fontFamily={fontFamily}
    >
      <FontApplier />
      <Flex as="header" align="center" py={5} px={4} gap={2}>
        <SettingsMenu />
        <Text fontWeight="600" letterSpacing="-0.02em">
          TodosToDo
        </Text>
      </Flex>

      <Box as="main">
        <EntryList />
      </Box>
    </Box>
  )
}
