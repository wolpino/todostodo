import type { Entry, EntryStatus } from '@/api/generated'

/**
 * The generated `Entry` type reflects what the API sends today — no `entryType`
 * discriminator yet. This file extends it manually so the rest of the frontend
 * can be written against the polymorphic shape now.
 *
 * When the backend adds `entryType` to the Swagger spec, re-run
 * `pnpm generate-api` and update `BaseEntry` to derive the field from the
 * generated type rather than declaring it here.
 */

export type { EntryStatus }

type BaseEntry = Entry & {
  /** TPH discriminator — matches the EF Core `EntryType` column. */
  entryType: string
}

export type TodoEntry = BaseEntry & {
  entryType: 'Todo'
  dueDate?: string
  dueTime?: string
}

export type EventEntry = BaseEntry & {
  entryType: 'Event'
}

export type NoteEntry = BaseEntry & {
  entryType: 'Note'
}

export type AnyEntry = TodoEntry | EventEntry | NoteEntry

// ---------------------------------------------------------------------------
// Type guards
// ---------------------------------------------------------------------------

export const isTodoEntry = (entry: AnyEntry): entry is TodoEntry =>
  entry.entryType === 'Todo'

export const isEventEntry = (entry: AnyEntry): entry is EventEntry =>
  entry.entryType === 'Event'

export const isNoteEntry = (entry: AnyEntry): entry is NoteEntry =>
  entry.entryType === 'Note'

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** The ordered status cycle used by the status toggle button. */
export const STATUS_CYCLE = [
  'Active',
  'InProgress',
  'Completed',
  'Archived',
  'Inactive',
] as const satisfies readonly EntryStatus[]

/** Returns the next status in the cycle, wrapping from Inactive back to Active. */
export const nextStatus = (current: EntryStatus): EntryStatus => {
  const idx = STATUS_CYCLE.indexOf(current)
  return STATUS_CYCLE[(idx + 1) % STATUS_CYCLE.length]
}

/**
 * Casts a raw API `Entry` to `AnyEntry`.
 * Defaults to `'Todo'` until the backend exposes `entryType` in the response.
 */
export const toAnyEntry = (raw: Entry): AnyEntry =>
  ({ entryType: 'Todo', ...raw }) as TodoEntry
