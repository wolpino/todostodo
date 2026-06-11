import { createFileRoute, redirect, useNavigate } from '@tanstack/react-router'
import { Box, Button, Flex, Text } from '@chakra-ui/react'
import { authQueryOptions, useLogout } from '@/hooks/useAuth'
import { EntryList } from '@/components/features/EntryList'

export const Route = createFileRoute('/')({
  beforeLoad: async ({ context }) => {
    const user = await context.queryClient.ensureQueryData(authQueryOptions)
    if (!user) throw redirect({ to: '/login' })
  },
  component: HomePage,
})

function HomePage() {
  const navigate = useNavigate()
  const logout = useLogout()

  return (
    <Box w="100%" maxW="500px" mx="auto" px={4} minH="100svh">
      <Flex as="header" justify="space-between" align="center" py={5}>
        <Text fontWeight="600" letterSpacing="-0.02em">
          todostodo
        </Text>
        <Button
          variant="ghost"
          size="sm"
          color="gray.500"
          onClick={() => logout.mutateAsync().then(() => navigate({ to: '/login' }))}
          loading={logout.isPending}
          loadingText="Signing out…"
        >
          Sign out
        </Button>
      </Flex>

      <Box as="main">
        <EntryList />
      </Box>
    </Box>
  )
}
