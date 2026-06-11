import { memo } from 'react'
import { Button } from '@chakra-ui/react'
import type { EntryStatus } from '@/types/entries'
import { nextStatus } from '@/types/entries'

const STATUS_LABELS: Record<EntryStatus, string> = {
  Active: 'Active',
  InProgress: 'In Progress',
  Completed: 'Completed',
  Archived: 'Archived',
  Inactive: 'Inactive',
}

/** Hex colors are intentionally hardcoded — these are semantic status colors,
 *  not theme tokens, and should be the same in light and dark mode. */
const STATUS_COLORS: Record<EntryStatus, string> = {
  Active: '#7c3aed',
  InProgress: '#f59e0b',
  Completed: '#22c55e',
  Archived: '#9ca3af',
  Inactive: '#f87171',
}

function StatusIconPaths({ status }: { status: EntryStatus }) {
  switch (status) {
    case 'Active':
      return (
        <circle cx="8" cy="8" r="5.5" fill="none" stroke="currentColor" strokeWidth="1.5" />
      )
    case 'InProgress':
      return (
        <>
          <circle cx="8" cy="8" r="5.5" fill="none" stroke="currentColor" strokeWidth="1.5" />
          {/* Right half filled — pie from 12 o'clock clockwise to 6 o'clock */}
          <path d="M 8 8 L 8 2.5 A 5.5 5.5 0 0 1 8 13.5 Z" fill="currentColor" />
        </>
      )
    case 'Completed':
      return (
        <>
        <>
          <circle cx="8" cy="8" r="5.5" fill="currentColor" stroke="currentColor" strokeWidth="1.5" />
        </>
        </>
      )
    case 'Archived':
      return <circle cx="8" cy="8" r="5.5" fill="currentColor" opacity="0.45" />
    case 'Inactive':
      return (
        <>
          <circle cx="8" cy="8" r="5.5" fill="none" stroke="currentColor" strokeWidth="1.5" />
          <line x1="5.5" y1="5.5" x2="10.5" y2="10.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
          <line x1="10.5" y1="5.5" x2="5.5" y2="10.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
        </>
      )
  }
}

type StatusButtonProps = {
  status: EntryStatus
  onCycle: () => void
  isLoading?: boolean
}

export const StatusButton = memo(function StatusButton({
  status,
  onCycle,
  isLoading = false,
}: StatusButtonProps) {
  const next = nextStatus(status)
  return (
    <Button
      type="button"
      variant="ghost"
      flexShrink={0}
      w="32px"
      h="32px"
      minW="32px"
      p={0}
      borderRadius="full"
      color={STATUS_COLORS[status]}
      aria-label={`Status: ${STATUS_LABELS[status]}. Click to set ${STATUS_LABELS[next]}`}
      onClick={onCycle}
      disabled={isLoading}
      cursor={isLoading ? 'wait' : 'pointer'}
      _hover={{ opacity: 0.7, bg: 'transparent' }}
    >
      <svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true">
        <StatusIconPaths status={status} />
      </svg>
    </Button>
  )
})
