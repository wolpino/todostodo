import { Box, Spinner, Text } from '@chakra-ui/react'
import { useEntries } from '@/hooks/useEntries'
import { isTodoEntry } from '@/types/entries'
import type { AnyEntry, EntryStatus } from '@/types/entries'
import { TodoItem } from './TodoItem'
import { EmptyRow } from './EmptyRow'

/** InProgress floats to the top; Archived sinks to the bottom. Everything else keeps insertion order. */
const STATUS_SORT_ORDER: Record<EntryStatus, number> = {
  InProgress: 0,
  Active: 1,
  Completed: 2,
  Archived: 3,
  Inactive: 4,
}

const sortEntries = (entries: AnyEntry[]): AnyEntry[] =>
  [...entries].sort(
    (a, b) =>
      (STATUS_SORT_ORDER[a.status ?? 'Active'] ?? 1) -
      (STATUS_SORT_ORDER[b.status ?? 'Active'] ?? 1),
  )

function EntryDispatcher({ entry }: { entry: AnyEntry }) {
  if (isTodoEntry(entry)) return <TodoItem entry={entry} />
  // EventItem and NoteItem are post-MVP — fall back to TodoItem for unknown types
  return <TodoItem entry={{ ...entry, kind: 'Todo' }} />
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

  const sorted = sortEntries(entries ?? [])

  return (
    <Box role="list" aria-label="Todo entries">
      <Box role="listitem">
        <EmptyRow />
      </Box>
      {sorted.map((entry) => (
        <Box key={entry.id} role="listitem">
          <EntryDispatcher entry={entry} />
        </Box>
      ))}
    </Box>
  )
}
