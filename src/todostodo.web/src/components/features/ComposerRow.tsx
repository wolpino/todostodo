import { useEffect, useRef, useState } from 'react'
import { Box, Input } from '@chakra-ui/react'
import { useCreateEntry } from '@/hooks/useEntries'
import { EntryRow } from './EntryRow'
import { StatusBullet } from './StatusBullet'

export const COMPOSER_INPUT_ID = 'entry-composer-input'

/** Always-visible composer at the top of the entry list. */
export function ComposerRow() {
  const [draft, setDraft] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)
  const skipBlurCommitRef = useRef(false)
  const createMutation = useCreateEntry()

  useEffect(() => {
    inputRef.current?.focus()
  }, [])

  const keepFocus = () => {
    requestAnimationFrame(() => inputRef.current?.focus())
  }

  const commit = (opts?: { keepFocus?: boolean }) => {
    const trimmed = draft.trim()
    if (!trimmed) return

    if (opts?.keepFocus) {
      skipBlurCommitRef.current = true
    }

    createMutation.mutate({ title: trimmed, status: 'Active' })
    setDraft('')

    if (opts?.keepFocus) {
      keepFocus()
      requestAnimationFrame(() => {
        skipBlurCommitRef.current = false
      })
    }
  }

  const handleBlur = () => {
    if (skipBlurCommitRef.current) return
    commit()
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault()
      commit({ keepFocus: true })
    } else if (e.key === 'Escape') {
      e.preventDefault()
      setDraft('')
      keepFocus()
    }
  }

  return (
    <EntryRow
      statusSlot={<StatusBullet faded />}
      deleteSlot={<Box aria-hidden="true" />}
    >
      <Input
        id={COMPOSER_INPUT_ID}
        ref={inputRef}
        value={draft}
        onChange={(e) => setDraft(e.target.value)}
        onBlur={handleBlur}
        onKeyDown={handleKeyDown}
        size="sm"
        fontSize="inherit"
        textAlign="left"
        placeholder="What needs doing?"
        _placeholder={{ color: 'gray.400' }}
        px={1}
        aria-busy={createMutation.isPending}
      />
    </EntryRow>
  )
}
