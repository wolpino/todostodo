import { useState } from 'react'
import { createFileRoute, redirect, useNavigate } from '@tanstack/react-router'
import {
  Box,
  Button,
  Input,
  Stack,
  Text,
} from '@chakra-ui/react'
import { authQueryOptions, useLogin, useRegister } from '@/hooks/useAuth'

export const Route = createFileRoute('/login')({
  beforeLoad: async ({ context }) => {
    const user = await context.queryClient.ensureQueryData(authQueryOptions)
    if (user) throw redirect({ to: '/' })
  },
  component: LoginPage,
})

type AuthMode = 'login' | 'register'

function LoginPage() {
  const [mode, setMode] = useState<AuthMode>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')

  const navigate = useNavigate()
  const loginMutation = useLogin()
  const registerMutation = useRegister()

  const isPending = loginMutation.isPending || registerMutation.isPending

  // Show the most recent error from either mutation
  const errorMessage = (() => {
    if (registerMutation.isError) return 'Registration failed. Try a stronger password (6+ chars, digit, uppercase).'
    if (loginMutation.isError) return 'Invalid email or password.'
    return null
  })()

  const handleSubmit = async (e: React.SubmitEvent) => {
    e.preventDefault()
    loginMutation.reset()
    registerMutation.reset()

    if (mode === 'register') {
      try {
        await registerMutation.mutateAsync({ email, password })
      } catch {
        return
      }
    }

    try {
      await loginMutation.mutateAsync({ email, password })
      await navigate({ to: '/' })
    } catch {
      // error displayed via loginMutation.isError
    }
  }

  const toggleMode = () => {
    setMode((m) => (m === 'login' ? 'register' : 'login'))
    loginMutation.reset()
    registerMutation.reset()
  }

  return (
    <Box maxW="360px" mx="auto" px={4} pt="20vh">
      <Text fontSize="2xl" fontWeight="600" letterSpacing="-0.03em" mb={8}>
        todostodo
      </Text>

      <Box as="form" onSubmit={handleSubmit}>
        <Stack gap={3}>
          <Input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
            size="md"
          />
          <Input
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
            size="md"
          />

          {errorMessage && (
            <Text color="red.500" fontSize="sm">
              {errorMessage}
            </Text>
          )}

          <Button
            type="submit"
            colorPalette="purple"
            loading={isPending}
            loadingText={mode === 'login' ? 'Signing in…' : 'Creating account…'}
            w="full"
          >
            {mode === 'login' ? 'Sign in' : 'Create account'}
          </Button>
        </Stack>
      </Box>

      <Text mt={5} fontSize="sm" color="gray.500" textAlign="center">
        {mode === 'login' ? "Don't have an account? " : 'Already have an account? '}
        <Button
          variant="plain"
          size="xs"
          color="purple.500"
          onClick={toggleMode}
          p={0}
          h="auto"
          fontWeight="normal"
          textDecoration="underline"
        >
          {mode === 'login' ? 'Sign up' : 'Sign in'}
        </Button>
      </Text>
    </Box>
  )
}
