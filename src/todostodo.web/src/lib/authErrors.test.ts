import { describe, expect, it } from 'vitest'
import { getLoginErrorMessage, getRegisterErrorMessage } from '@/lib/authErrors'

describe('getRegisterErrorMessage', () => {
  it('returns a friendly message for duplicate email', () => {
    const message = getRegisterErrorMessage({
      errors: {
        DuplicateUserName: ["Username 'taken@example.com' is already taken."],
      },
    })

    expect(message).toBe('That email is already registered. Sign in instead.')
  })

  it('returns password validation messages from the API', () => {
    const message = getRegisterErrorMessage({
      errors: {
        PasswordTooShort: ['Passwords must be at least 6 characters.'],
        PasswordRequiresDigit: ["Passwords must have at least one digit ('0'-'9')."],
      },
    })

    expect(message).toContain('6 characters')
    expect(message).toContain('digit')
  })
})

describe('getLoginErrorMessage', () => {
  it('returns a generic auth failure message for 401', () => {
    expect(getLoginErrorMessage(401)).toBe('Incorrect email or password.')
  })
})
