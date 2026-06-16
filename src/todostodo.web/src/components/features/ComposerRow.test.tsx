import type { ReactNode } from 'react'
import { render, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ChakraProvider, defaultSystem } from '@chakra-ui/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { COMPOSER_INPUT_ID, ComposerRow } from './ComposerRow'

const mutateMock = vi.fn()

vi.mock('@/hooks/useEntries', () => ({
  useCreateEntry: () => ({
    mutate: mutateMock,
    isPending: false,
  }),
}))

function Wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  return (
    <QueryClientProvider client={queryClient}>
      <ChakraProvider value={defaultSystem}>{children}</ChakraProvider>
    </QueryClientProvider>
  )
}

function getComposerInput() {
  const input = document.getElementById(COMPOSER_INPUT_ID)
  if (!(input instanceof HTMLInputElement)) {
    throw new Error(`Expected #${COMPOSER_INPUT_ID} to be an input`)
  }
  return input
}

describe('ComposerRow', () => {
  beforeEach(() => {
    mutateMock.mockClear()
  })

  it('focuses the input on mount', async () => {
    render(<ComposerRow />, { wrapper: Wrapper })

    await waitFor(() => expect(getComposerInput()).toHaveFocus())
  })

  it('creates an entry and clears the draft on Enter', async () => {
    const user = userEvent.setup()
    render(<ComposerRow />, { wrapper: Wrapper })

    const input = getComposerInput()
    await user.type(input, 'Buy milk{Enter}')

    expect(mutateMock).toHaveBeenCalledWith({ title: 'Buy milk', status: 'Active' })
    expect(input).toHaveValue('')
  })

  it('keeps focus on the input after Enter', async () => {
    const user = userEvent.setup()
    render(<ComposerRow />, { wrapper: Wrapper })

    const input = getComposerInput()
    await user.type(input, 'Buy milk{Enter}')

    expect(input).toHaveFocus()
  })

  it('allows typing the next entry immediately after Enter', async () => {
    const user = userEvent.setup()
    render(<ComposerRow />, { wrapper: Wrapper })

    const input = getComposerInput()
    await user.type(input, 'First{Enter}Second')

    expect(mutateMock).toHaveBeenCalledWith({ title: 'First', status: 'Active' })
    expect(input).toHaveValue('Second')
    expect(input).toHaveFocus()
  })

  it('creates an entry on blur when the draft is non-empty', async () => {
    const user = userEvent.setup()
    render(<ComposerRow />, { wrapper: Wrapper })

    const input = getComposerInput()
    await user.type(input, 'Buy milk')
    await user.tab()

    expect(mutateMock).toHaveBeenCalledWith({ title: 'Buy milk', status: 'Active' })
    expect(input).toHaveValue('')
  })

  it('clears the draft on Escape and keeps focus', async () => {
    const user = userEvent.setup()
    render(<ComposerRow />, { wrapper: Wrapper })

    const input = getComposerInput()
    await user.type(input, 'draft text{Escape}')

    expect(mutateMock).not.toHaveBeenCalled()
    expect(input).toHaveValue('')
    expect(input).toHaveFocus()
  })
})
