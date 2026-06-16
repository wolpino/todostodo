import type { ReactNode } from 'react'
import { Box, Flex } from '@chakra-ui/react'

type EntryRowProps = {
  /** Left slot — StatusButton or StatusBullet */
  statusSlot: ReactNode
  /** Right slot — DeleteButton; pass a spacer to preserve layout */
  deleteSlot: ReactNode
  children: ReactNode
}

/** Shared layout shell for composer and entry rows. */
export function EntryRow({ statusSlot, deleteSlot, children }: EntryRowProps) {
  return (
    <Flex
      align="center"
      minH="44px"
      px={4}
      gap={1}
      borderBottom="1px solid"
      borderColor="gray.200"
      _dark={{ borderColor: 'gray.700' }}
      className="group"
      _hover={{ bg: 'gray.50', _dark: { bg: 'gray.800' } }}
      transition="background-color 0.1s"
    >
      {statusSlot}

      <Box flex={1} minW={0} py={1}>
        {children}
      </Box>

      <Box
        w="32px"
        h="32px"
        flexShrink={0}
        display="flex"
        alignItems="center"
        justifyContent="center"
      >
        {deleteSlot}
      </Box>
    </Flex>
  )
}
