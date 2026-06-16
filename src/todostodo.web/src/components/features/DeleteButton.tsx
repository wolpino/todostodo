import { memo } from 'react'
import { Button } from '@chakra-ui/react'

type DeleteButtonProps = {
  onDelete: () => void
  isLoading?: boolean
}

export const DeleteButton = memo(function DeleteButton({
  onDelete,
  isLoading = false,
}: DeleteButtonProps) {
  return (
    <Button
      type="button"
      variant="ghost"
      flexShrink={0}
      w="32px"
      h="32px"
      minW="32px"
      p={0}
      borderRadius="md"
      color="gray.400"
      aria-label="Delete entry"
      opacity={0}
      transition="opacity 0.1s"
      _groupHover={{ opacity: 1 }}
      _groupFocusWithin={{ opacity: 1 }}
      _hover={{ opacity: 1, color: 'red.500', bg: 'red.50', _dark: { bg: 'red.900', color: 'red.300' } }}
      _focusVisible={{
        opacity: 1,
        outline: '2px solid',
        outlineColor: 'blue.500',
        outlineOffset: '2px',
      }}
      onClick={onDelete}
      disabled={isLoading}
      cursor={isLoading ? 'wait' : 'pointer'}
    >
      <svg width="10" height="10" viewBox="0 0 10 10" fill="none" aria-hidden="true">
        <line x1="1" y1="1" x2="9" y2="9" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
        <line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
      </svg>
    </Button>
  )
})
