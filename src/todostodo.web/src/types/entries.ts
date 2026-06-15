import type { Entry, EntryStatus } from '@/api/generated'

/**
 * The generated `Entry` type reflects what the API sends today — no `kind`
 * discriminator yet. This file extends it manually so the rest of the frontend
 * can be written against the polymorphic shape now.
 *
 * When the backend adds `kind` to the Swagger spec, re-run
 * `pnpm generate-api` and update `BaseEntry` to derive the field from the
 * generated type rather than declaring it here.
 */

export type { EntryStatus }

type BaseEntry = Entry & {
  kind: string
}

export type TodoEntry = BaseEntry & {
  kind: 'Todo'
  dueDate?: string
  dueTime?: string
}

export type EventEntry = BaseEntry & {
  kind: 'Event'
}

export type NoteEntry = BaseEntry & {
  kind: 'Note'
}

export type AnyEntry = TodoEntry | EventEntry | NoteEntry

// ---------------------------------------------------------------------------
// Type guards
// ---------------------------------------------------------------------------

export const isTodoEntry = (entry: AnyEntry): entry is TodoEntry =>
  entry.kind === 'Todo'

export const isEventEntry = (entry: AnyEntry): entry is EventEntry =>
  entry.kind === 'Event'

export const isNoteEntry = (entry: AnyEntry): entry is NoteEntry =>
  entry.kind === 'Note'

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

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

/**
 * Casts a raw API `Entry` to `AnyEntry`.
 * Defaults to `'Todo'` .
 */
export const toAnyEntry = (raw: Entry): AnyEntry =>
  ({ ...raw, kind: raw.kind ?? 'Todo' }) as TodoEntry
