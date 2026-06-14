import { memo, useRef, useState } from 'react'
import { Input, Text } from '@chakra-ui/react'
import { EntryRow } from './EntryRow'
import { StatusButton } from './StatusButton'
import { DeleteButton } from './DeleteButton'
import { useUpdateEntry, useDeleteEntry } from '@/hooks/useEntries'
import { nextStatus } from '@/types/entries'
import type { TodoEntry } from '@/types/entries'

type TodoItemProps = {
  entry: TodoEntry
}

export const TodoItem = memo(function TodoItem({ entry }: TodoItemProps) {
  const [isEditing, setIsEditing] = useState(false)
  const [draft, setDraft] = useState(entry.title ?? '')
  const inputRef = useRef<HTMLInputElement>(null)

  const updateMutation = useUpdateEntry()
  const deleteMutation = useDeleteEntry()

  const id = entry.id!

  const handleTextClick = () => {
    setDraft(entry.title ?? '')
    setIsEditing(true)
    // Focus handled by autoFocus on Input
  }

  const commitEdit = () => {
    const trimmed = draft.trim()
    if (trimmed !== (entry.title ?? '').trim()) {
      updateMutation.mutate({ id, title: trimmed || null })
    }
    setIsEditing(false)
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      inputRef.current?.blur()
    } else if (e.key === 'Escape') {
      setDraft(entry.title ?? '')
      setIsEditing(false)
    }
  }

  const handleCycleStatus = () => {
    if (!entry.status) return
    updateMutation.mutate({ id, status: nextStatus(entry.status) })
  }

  return (
    <EntryRow
      statusSlot={
        <StatusButton
          status={entry.status ?? 'Active'}
          onCycle={handleCycleStatus}
          isLoading={updateMutation.isPending}
        />
      }
      deleteSlot={
        <DeleteButton
          onDelete={() => deleteMutation.mutate(id)}
          isLoading={deleteMutation.isPending}
        />
      }
    >
      {isEditing ? (
        <Input
          ref={inputRef}
          autoFocus
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onBlur={commitEdit}
          onKeyDown={handleKeyDown}
          size="sm"
          fontSize="inherit"
          textAlign="left"
          placeholder="What needs doing?"
          _placeholder={{ color: 'gray.400' }}
          px={1}
        />
      ) : (
        <Text
          as="span"
          display="block"
          textAlign="left"
          px={1}
          py="2px"
          fontSize="inherit"
          // <TODO> add more styling for different statuses
          color={entry.status === 'Completed' ? 'gray.400' : 'inherit'}
          textDecoration={entry.status === 'Completed' ? 'line-through' : 'none'}
          cursor="text"
          userSelect="none"
          onClick={handleTextClick}
          fontWeight="bold"
        >
          {entry.title ?? (
            <Text as="span" color="gray.400" fontStyle="italic">
              Untitled
            </Text>
          )}
        </Text>
      )}
    </EntryRow>
  )
})
