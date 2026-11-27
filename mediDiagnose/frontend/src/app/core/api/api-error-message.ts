import { HttpErrorResponse } from '@angular/common/http';

export function apiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (error.status === 0) {
    return 'The hospital service could not be reached. Check your connection and try again.';
  }

  const body: unknown = error.error;
  if (typeof body === 'object' && body !== null) {
    const response = body as Record<string, unknown>;
    if (typeof response['message'] === 'string') {
      return response['message'];
    }
    const errors = response['errors'];
    if (typeof errors === 'object' && errors !== null) {
      const messages = Object.values(errors)
        .flatMap((value) => (Array.isArray(value) ? value : []))
        .filter((value): value is string => typeof value === 'string');
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }
  }

  return fallback;
}
