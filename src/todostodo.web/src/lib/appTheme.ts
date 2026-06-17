import type { IconButtonProps } from '@chakra-ui/react'

/** Matches login title and notebook border (`green.solid` in Chakra). */
export const APP_GREEN = 'green.600'

export const headerIconButtonProps: IconButtonProps = {
  variant: 'ghost',
  boxSize: '64px',
  minW: '64px',
  color: APP_GREEN,
  _hover: { color: APP_GREEN, bg: 'green.50', _dark: { bg: 'green.950' } },
}

export const HEADER_ICON_SLOT = '64px'
