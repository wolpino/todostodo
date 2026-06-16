import { Box } from '@chakra-ui/react'

type StatusBulletProps = {
  /** Faded grey for decorative rows (e.g. composer placeholder). */
  faded?: boolean
}

/** Decorative status circle — matches Active `StatusButton` icon, non-interactive. */
export function StatusBullet({ faded = false }: StatusBulletProps) {
  return (
    <Box
      flexShrink={0}
      w="32px"
      h="32px"
      display="flex"
      alignItems="center"
      justifyContent="center"
      color="currentColor"
      opacity={faded ? 0.5 : 1}
      aria-hidden="true"
    >
      <svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true">
        <circle cx="8" cy="8" r="7.1" fill="none" stroke="currentColor" strokeWidth="1.5" />
      </svg>
    </Box>
  )
}
