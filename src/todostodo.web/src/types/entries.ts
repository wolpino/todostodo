import type { Entry as ApiEntry, EntryKind, EntryStatus } from '@/api/generated'

export type { EntryKind, EntryStatus }

/**
 * Polymorphic entry shapes for the frontend. The API returns a flat `Entry`
 * with a `kind` field (Todo | Note | Event). MVP only renders Todo; other kinds
 * are typed here for future row components.
 */
export type TodoEntry = ApiEntry & { kind: 'Todo' }
export type EventEntry = ApiEntry & { kind: 'Event' }
export type NoteEntry = ApiEntry & { kind: 'Note' }
export type AnyEntry = TodoEntry | EventEntry | NoteEntry

export const isTodoEntry = (entry: AnyEntry): entry is TodoEntry =>
  entry.kind === 'Todo'

export const isEventEntry = (entry: AnyEntry): entry is EventEntry =>
  entry.kind === 'Event'

export const isNoteEntry = (entry: AnyEntry): entry is NoteEntry =>
  entry.kind === 'Note'

/** The ordered status cycle used by the status toggle button. Stops at Archived. */
export const STATUS_CYCLE = [
  'Active',
  'InProgress',
  'Completed',
  'Archived',
] as const satisfies readonly EntryStatus[]

/**
 * Returns the next status in the cycle.
 * Stops at Archived — clicking beyond it does nothing.
 * Inactive is only reachable via the delete button, not this cycle.
 */
export const nextStatus = (current: EntryStatus): EntryStatus => {
  const idx = STATUS_CYCLE.indexOf(current as (typeof STATUS_CYCLE)[number])
  if (idx === -1 || idx === STATUS_CYCLE.length - 1) return current
  return STATUS_CYCLE[idx + 1]
}

/** Normalize API entries for components; defaults missing kind to Todo. */
export const toAnyEntry = (raw: ApiEntry): AnyEntry =>
  ({ ...raw, kind: raw.kind ?? 'Todo' }) as TodoEntry
