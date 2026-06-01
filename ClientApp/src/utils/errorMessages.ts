// User-friendly error messages mapped to error types
export const ERROR_MESSAGES = {
  // HTTP Status Errors
  BAD_REQUEST: 'Bad Request',
  UNAUTHORIZED: 'You are not authorized to perform this action',
  FORBIDDEN: 'You do not have permission to access this resource',
  NOT_FOUND: 'The requested resource was not found',
  CONFLICT: 'This resource already exists',
  UNPROCESSABLE_ENTITY: 'The data provided could not be processed',
  INTERNAL_SERVER_ERROR: 'An internal server error occurred',
  BAD_GATEWAY: 'The server is temporarily unavailable',
  SERVICE_UNAVAILABLE: 'The service is currently unavailable',

  // ToDo -specific Errors
  TODO_NOT_FOUND: 'Todo not found',
  TODO_CREATE_FAILED: 'Failed to create todo',
  TODO_UPDATE_FAILED: 'Failed to update todo',
  TODO_DELETE_FAILED: 'Failed to delete todo',
  TODO_FETCH_FAILED: 'Failed to fetch todo',

  // Validation Errors
  VALIDATION_FAILED: 'Please check your input and try again',
  REQUIRED_FIELD: 'This field is required',
  INVALID_FORMAT: 'The format is invalid',

  // Network Errors
  NETWORK_ERROR: 'Network error. Please check your connection',
  TIMEOUT_ERROR: 'Request timed out. Please try again',

  // Generic Error
  GENERIC_ERROR: 'Something went wrong. Please try again',
} as const;

export type ErrorMessageKey = keyof typeof ERROR_MESSAGES;
