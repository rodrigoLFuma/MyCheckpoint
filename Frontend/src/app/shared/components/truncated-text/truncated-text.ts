import { Component, input } from '@angular/core';

const DEFAULT_LIMIT = 30;

/**
 * Texto que se recorta a `limit` caracteres con "…" si hace falta. Puramente
 * visual, sin interacción propia: se usa siempre dentro de una tarjeta que ya
 * es un enlace a la reseña completa (ReviewCard, Mi biblioteca), así que
 * "ver el resto" ya lo resuelve pulsar en cualquier parte de la tarjeta.
 */
@Component({
  selector: 'app-truncated-text',
  templateUrl: './truncated-text.html',
})
export class TruncatedText {
  text = input<string | null | undefined>(null);
  limit = input<number>(DEFAULT_LIMIT);

  protected get visibleText(): string {
    const text = this.text() ?? '';
    if (text.length <= this.limit()) return text;
    return text.slice(0, this.limit()) + '…';
  }
}
