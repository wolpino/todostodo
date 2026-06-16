import { useRef, useState } from 'react'
import { Box, Input } from '@chakra-ui/react'
import { useCreateEntry } from '@/hooks/useEntries'
import { EntryRow } from './EntryRow'
import { StatusBullet } from './StatusBullet'

/**
 * Shown at the top of the entry list.
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
    <EntryRow
      statusSlot={<StatusBullet faded />}
      deleteSlot={<Box aria-hidden="true" />}
      onClick={isEditing ? undefined : handleRowClick}
      cursor={isEditing ? 'default' : 'text'}
    >
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
    </EntryRow>
  )
}
