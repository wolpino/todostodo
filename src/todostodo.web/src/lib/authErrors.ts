import type { HttpValidationProblemDetails } from '@/api/generated'
import { HttpError } from '@/lib/httpError'

const isProblemDetails = (body: unknown): body is HttpValidationProblemDetails =>
  typeof body === 'object' && body !== null && 'errors' in body

/** Maps ASP.NET Identity register validation errors to user-facing copy. */
export const getRegisterErrorMessage = (body: unknown): string => {
  if (!isProblemDetails(body) || !body.errors) {
    return 'Registration failed. Please try again.'
  }

  const { errors } = body

  if (errors.DuplicateUserName?.length || errors.DuplicateEmail?.length) {
    return 'That email is already registered. Sign in instead.'
  }

  const passwordMessages = [
    ...(errors.PasswordTooShort ?? []),
    ...(errors.PasswordRequiresDigit ?? []),
    ...(errors.PasswordRequiresUpper ?? []),
    ...(errors.PasswordRequiresLower ?? []),
    ...(errors.PasswordRequiresNonAlphanumeric ?? []),
  ]

  if (passwordMessages.length > 0) {
    return passwordMessages.join(' ')
  }

  const first = Object.values(errors).flat()[0]
  return first ?? 'Registration failed. Please try again.'
}

export const getLoginErrorMessage = (status: number): string => {
  if (status === 401) return 'Incorrect email or password.'
  return 'Sign in failed. Please try again.'
}

export const authErrorMessage = (err: unknown, fallback: string): string =>
  err instanceof HttpError ? err.message : fallback

export const PASSWORD_REQUIREMENTS_HINT =
  'At least 6 characters, with an uppercase letter, a digit, and a special character.'
