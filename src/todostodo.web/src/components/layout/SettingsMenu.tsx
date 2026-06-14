import { useNavigate } from '@tanstack/react-router'
import {
  Box,
  Button,
  Drawer,
  IconButton,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useLogout } from '@/hooks/useAuth'
import { useSettings, useUpdateSettings } from '@/hooks/useSettings'
import { DEFAULT_FONT, FONT_OPTIONS, type FontId } from '@/lib/fonts'

function GearIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path
        d="M12 15.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Z"
        stroke="currentColor"
        strokeWidth="1.5"
      />
      <path
        d="M19.4 13.5a7.9 7.9 0 0 0 .1-3l2-1.2-2-3.5-2.3.7a8 8 0 0 0-2.6-1.5L14 2h-4l-.6 2.9a8 8 0 0 0-2.6 1.5l-2.3-.7-2 3.5 2 1.2a7.9 7.9 0 0 0 0 3l-2 1.2 2 3.5 2.3-.7a8 8 0 0 0 2.6 1.5L10 22h4l.6-2.9a8 8 0 0 0 2.6-1.5l2.3.7 2-3.5-2-1.2Z"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinejoin="round"
      />
    </svg>
  )
}

export function SettingsMenu() {
  const navigate = useNavigate()
  const logout = useLogout()
  const { data: settings } = useSettings()
  const updateSettings = useUpdateSettings()

  const activeFont = (settings?.font as FontId | undefined) ?? DEFAULT_FONT

  const handleFontSelect = (font: FontId) => {
    if (font === activeFont) return
    updateSettings.mutate({ font })
  }

  const handleSignOut = async () => {
    await logout.mutateAsync()
    navigate({ to: '/login' })
  }

  return (
    <Drawer.Root>
      <Drawer.Trigger asChild>
        <IconButton
          aria-label="Open settings"
          variant="ghost"
          size="sm"
          color="gray.600"
          _dark={{ color: 'gray.300' }}
        >
          <GearIcon />
        </IconButton>
      </Drawer.Trigger>

      <Drawer.Backdrop />
      <Drawer.Positioner alignItems="flex-end">
        <Drawer.Content
          w="100%"
          borderTopRadius="xl"
          maxH="45vh"
          pb="env(safe-area-inset-bottom)"
        >
          <Drawer.Header borderBottomWidth="1px" borderColor="gray.200" _dark={{ borderColor: 'gray.700' }}>
            <Drawer.Title fontSize="md" fontWeight="600">
              Settings
            </Drawer.Title>
            <Drawer.CloseTrigger />
          </Drawer.Header>

          <Drawer.Body py={4}>
            <Text fontSize="sm" color="gray.500" mb={2}>
              Font
            </Text>
            <Stack gap={2}>
              {FONT_OPTIONS.map((option) => {
                const isActive = option.id === activeFont
                return (
                  <Button
                    key={option.id}
                    variant={isActive ? 'subtle' : 'outline'}
                    justifyContent="flex-start"
                    fontFamily={option.stack}
                    onClick={() => handleFontSelect(option.id)}
                    loading={updateSettings.isPending && updateSettings.variables?.font === option.id}
                  >
                    {option.label}
                  </Button>
                )
              })}
            </Stack>

            <Box mt={6} pt={4} borderTopWidth="1px" borderColor="gray.200" _dark={{ borderColor: 'gray.700' }}>
              <Button
                variant="ghost"
                color="gray.600"
                _dark={{ color: 'gray.300' }}
                w="full"
                justifyContent="flex-start"
                onClick={handleSignOut}
                loading={logout.isPending}
                loadingText="Signing out…"
              >
                Sign out
              </Button>
            </Box>
          </Drawer.Body>
        </Drawer.Content>
      </Drawer.Positioner>
    </Drawer.Root>
  )
}
