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
      onClick={onDelete}
      disabled={isLoading}
      cursor={isLoading ? 'wait' : 'pointer'}
      _hover={{ color: 'red.500', bg: 'red.50', _dark: { bg: 'red.900', color: 'red.300' } }}
    >
      {/* Heavy lined X per PRD */}
      <svg width="12" height="12" viewBox="0 0 12 12" fill="none" aria-hidden="true">
        <line x1="1.5" y1="1.5" x2="10.5" y2="10.5" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
        <line x1="10.5" y1="1.5" x2="1.5" y2="10.5" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      </svg>
    </Button>
  )
})
