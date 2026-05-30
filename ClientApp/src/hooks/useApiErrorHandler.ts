import { useState, useCallback } from 'react';
import { parseApiError, formatValidationErrors, type ParsedApiError } from './useApiError';

/**
 * React hook for managing API error state with user-friendly messages
 * @example
 * const { error, setError, getDisplayMessage } = useApiError();
 * try {
 *   await TasksService.getApiTasks();
 * } catch (err) {
 *   setError(err);
 *   console.log(getDisplayMessage()); // User-friendly message
 * }
 */
export function useApiErrorHandler() {
  const [error, setError] = useState<ParsedApiError | null>(null);

  const handleError = useCallback((err: unknown) => {
    const parsed = parseApiError(err);
    setError(parsed);
    return parsed;
  }, []);

  const getDisplayMessage = useCallback((): string => {
    if (!error) return '';
    
    // If there are validation errors, format them
    if (error.validationErrors) {
      return formatValidationErrors(error.validationErrors);
    }

    return error.message;
  }, [error]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    error,
    setError: handleError,
    getDisplayMessage,
    clearError,
    hasError: !!error,
  };
}
