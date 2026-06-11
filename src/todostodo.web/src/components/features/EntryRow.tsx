import type { ReactNode } from 'react'
import { Box, Flex } from '@chakra-ui/react'

type EntryRowProps = {
  /** Left slot — StatusButton for real entries */
  statusSlot: ReactNode
  /** Right slot — DeleteButton for real entries */
  deleteSlot: ReactNode
  children: ReactNode
}

/**
 * Shared layout shell for all entry types.
 * Delete button is hidden until the row is hovered.
 */
export function EntryRow({ statusSlot, deleteSlot, children }: EntryRowProps) {
  return (
    <Flex
      align="center"
      minH="44px"
      px={1}
      gap={1}
      borderBottom="1px solid"
      borderColor="gray.200"
      _dark={{ borderColor: 'gray.700' }}
      data-group
      _hover={{ bg: 'gray.50', _dark: { bg: 'gray.800' } }}
      transition="background-color 0.1s"
    >
      {statusSlot}

      <Box flex={1} minW={0} py={1}>
        {children}
      </Box>

      {/* Show faded delete button by default, on hover it becomes black with thicker lines */}
      <Box>
        {deleteSlot}
      </Box>
 
    </Flex>
  )
}
