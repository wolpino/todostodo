import { memo, useRef, useState } from 'react'
import { Input, Text } from '@chakra-ui/react'
import { useQueryClient } from '@tanstack/react-query'
import { EntryRow } from './EntryRow'
import { StatusButton } from './StatusButton'
import { DeleteButton } from './DeleteButton'
import { ENTRIES_QUERY_KEY, useUpdateEntry, useDeleteEntry } from '@/hooks/useEntries'
import { nextStatus } from '@/types/entries'
import type { AnyEntry, TodoEntry } from '@/types/entries'

type TodoItemProps = {
  entry: TodoEntry
}

export const TodoItem = memo(function TodoItem({ entry }: TodoItemProps) {
  const [isEditing, setIsEditing] = useState(false)
  const [draft, setDraft] = useState(entry.title ?? '')
  const inputRef = useRef<HTMLInputElement>(null)
  const queryClient = useQueryClient()

  const updateMutation = useUpdateEntry()
  const deleteMutation = useDeleteEntry()

  const id = entry.id!

  const isStatusUpdating =
    updateMutation.isPending &&
    updateMutation.variables?.id === id &&
    updateMutation.variables.status !== undefined

  const status = entry.status ?? 'Active'
  const isDone = status === 'Completed' || status === 'Archived'
  const isOpen = status === 'Active' || status === 'InProgress'

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
    const entries = queryClient.getQueryData<AnyEntry[]>(ENTRIES_QUERY_KEY)
    const current = entries?.find((e) => e.id === id)?.status ?? entry.status ?? 'Active'
    const next = nextStatus(current)
    if (next === current) return
    updateMutation.mutate({ id, status: next })
  }

  return (
    <EntryRow
      statusSlot={
        <StatusButton
          status={status}
          onCycle={handleCycleStatus}
          isLoading={isStatusUpdating}
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
          opacity={status === 'Archived' ? 0.4 : status === 'Completed' ? 0.65 : 1}
          fontStyle={status === 'Archived' ? 'italic' : 'normal'}
          textDecoration={isDone ? 'line-through' : 'none'}
          cursor={status === 'Archived' ? 'default' : 'text'}
          userSelect="none"
          onClick={status === 'Archived' ? undefined : handleTextClick}
          fontWeight={isOpen ? 'bold' : 'normal'}
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
