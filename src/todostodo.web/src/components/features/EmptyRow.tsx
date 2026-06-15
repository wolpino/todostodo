import { useRef, useState } from 'react'
import { Box, Flex, Input } from '@chakra-ui/react'
import { useCreateEntry } from '@/hooks/useEntries'

/**
 * Shown when the entry list is empty.
 * Looks like a row but has a decorative non-status bullet.
 * Clicking reveals an input; blurring with text creates a new entry.
 */
export function EmptyRow() {
  const [isEditing, setIsEditing] = useState(false)
  const [draft, setDraft] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)
  const createMutation = useCreateEntry()

  const handleRowClick = () => {
    if (!isEditing) setIsEditing(true)
  }

  const commit = () => {
    const trimmed = draft.trim()
    if (trimmed) {
      createMutation.mutate({ title: trimmed, status: 'Active' })
      setDraft('')
    }
    setIsEditing(false)
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      const trimmed = draft.trim()
      if (trimmed) {
        // TODO ideally on enter the next input is focused and a user can type wihtout selecting
        // createMutation.mutate({ title: trimmed, status: 'Active' })
        // setDraft('')
        inputRef.current?.blur()
      }

    } else if (e.key === 'Escape') {
      setDraft('')
      setIsEditing(false)
    }
  }

  return (
    <Flex
      align="center"
      minH="44px"
      px={1}
      gap={1}
      borderBottom="1px solid"
      borderColor="gray.200"
      _dark={{ borderColor: 'gray.700' }}
      cursor={isEditing ? 'default' : 'text'}
      onClick={handleRowClick}
      _hover={{ bg: 'gray.50', _dark: { bg: 'gray.800' } }}
      transition="background-color 0.1s"
    >
      {/* Decorative bullet — not a status button */}
      <Box
        flexShrink={0}
        w="32px"
        h="32px"
        display="flex"
        alignItems="center"
        justifyContent="center"
        aria-hidden="true"
      >
        <Box
          w="10px"
          h="10px"
          borderRadius="full"
          border="1.5px solid"
          borderColor="gray.300"
          opacity={0.5}
          _dark={{ borderColor: 'gray.500' }}
        />
      </Box>

      <Box flex={1} minW={0} py={1}>
        {isEditing ? (
          <Input
            ref={inputRef}
            autoFocus
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            onBlur={commit}
            onKeyDown={handleKeyDown}
            size="sm"
            fontSize="inherit"
            textAlign="left"
            placeholder="What needs doing?"
            _placeholder={{ color: 'gray.400' }}
            px={1}
            disabled={createMutation.isPending}
          />
        ) : (
          <Box px={1} color="gray.400" fontSize="inherit" fontStyle="italic" userSelect="none">
            Add a todo…
          </Box>
        )}
      </Box>
    </Flex>
  )
}
