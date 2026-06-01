import { ApiError } from '../api/core/ApiError';
import { ERROR_MESSAGES, type ErrorMessageKey } from '../utils/errorMessages';

export interface ParsedApiError {
  message: string;
  status: number;
  statusText: string;
  validationErrors?: Record<string, string[]>;
}

/**
 * Parse an ApiError and return a user-friendly message
 * Handles both generic errors and validation problem details from C# API
 */
export function parseApiError(error: unknown): ParsedApiError {
  // Handle ApiError
  if (error instanceof ApiError) {
    const parsedError: ParsedApiError = {
      message: getErrorMessage(error.status),
      status: error.status,
      statusText: error.statusText,
    };

    // Check if body contains validation errors (C# ValidationProblemDetails format)
    if (error.body?.errors && typeof error.body.errors === 'object') {
      parsedError.validationErrors = error.body.errors;
    }

    return parsedError;
  }

  // Handle generic errors
  if (error instanceof Error) {
    return {
      message: error.message,
      status: 0,
      statusText: 'Unknown Error',
    };
  }

  // Handle unknown errors
  return {
    message: ERROR_MESSAGES.GENERIC_ERROR,
    status: 0,
    statusText: 'Unknown Error',
  };
}

/**
 * Get a user-friendly error message based on HTTP status code
 */
function getErrorMessage(status: number): string {
  const messageMap: Record<number, ErrorMessageKey> = {
    400: 'BAD_REQUEST',
    401: 'UNAUTHORIZED',
    403: 'FORBIDDEN',
    404: 'NOT_FOUND',
    409: 'CONFLICT',
    422: 'UNPROCESSABLE_ENTITY',
    500: 'INTERNAL_SERVER_ERROR',
    502: 'BAD_GATEWAY',
    503: 'SERVICE_UNAVAILABLE',
  };

  const key = messageMap[status];
  return key ? ERROR_MESSAGES[key] : ERROR_MESSAGES.GENERIC_ERROR;
}

/**
 * Format validation errors into a single readable string
 */
export function formatValidationErrors(
  validationErrors?: Record<string, string[]>
): string {
  if (!validationErrors) {
    return ERROR_MESSAGES.VALIDATION_FAILED;
  }

  const errorMessages = Object.entries(validationErrors)
    .flatMap(([field, errors]) => errors.map((error) => error))
    .slice(0, 3); // Show max 3 errors

  return errorMessages.length > 0
    ? errorMessages.join('. ')
    : ERROR_MESSAGES.VALIDATION_FAILED;
}
