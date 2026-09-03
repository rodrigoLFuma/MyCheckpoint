import { HttpErrorResponse } from '@angular/common/http';

/**
 * Extrae el `detail` de un error de la API (formato ProblemDetails).
 *
 * Los endpoints que devuelven 204 No Content piden `responseType: 'text'`,
 * así que Angular no parsea el cuerpo del error como JSON automáticamente
 * (solo lo hace cuando responseType es 'json'): `err.error` llega como texto
 * plano, no como objeto. Hay que intentar parsearlo a mano.
 */
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
