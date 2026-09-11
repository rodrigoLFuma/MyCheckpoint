import { HttpErrorResponse } from '@angular/common/http';

export function extractErrorDetail(err: unknown): string | null {
  if (!(err instanceof HttpErrorResponse)) return null;
  const raw = err.error;
  if (raw && typeof raw === 'object') return raw.detail ?? null;
  if (typeof raw === 'string') {
    try {
      return JSON.parse(raw)?.detail ?? null;
    } catch {
      return null;
    }
  }
  return null;
}
