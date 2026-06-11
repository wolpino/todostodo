import { Box, Spinner, Text } from '@chakra-ui/react'
import { useEntries } from '@/hooks/useEntries'
import { isTodoEntry } from '@/types/entries'
import type { AnyEntry } from '@/types/entries'
import { TodoItem } from './TodoItem'
import { EmptyRow } from './EmptyRow'

function EntryDispatcher({ entry }: { entry: AnyEntry }) {
  if (isTodoEntry(entry)) return <TodoItem entry={entry} />
  // EventItem and NoteItem are post-MVP — fall back to TodoItem for unknown types
  return <TodoItem entry={{ ...entry, entryType: 'Todo' }} />
}

export function EntryList() {
  const { data: entries, isPending, isError } = useEntries()

  if (isPending) {
    return (
      <Box display="flex" justifyContent="center" py={8}>
        <Spinner size="sm" color="gray.400" />
      </Box>
    )
  }

  if (isError) {
    return (
      <Text color="red.500" px={2} py={4} fontSize="sm">
        Failed to load entries. Please refresh.
      </Text>
    )
  }

  return (
    <Box role="list" aria-label="Todo entries">
      <Box role="listitem">
        <EmptyRow />
      </Box>
      {entries?.map((entry) => (
        <Box key={entry.id} role="listitem">
          <EntryDispatcher entry={entry} />
        </Box>
      ))}
    </Box>
  )
}
