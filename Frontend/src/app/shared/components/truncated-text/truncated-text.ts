import { Component, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

const DEFAULT_LIMIT = 30;

/**
 * Texto que se recorta a `limit` caracteres con un botón "Ver más" si hace
 * falta. Si se le pasa `reviewId`, "Ver más" navega a la vista completa de
 * esa reseña (/reviews/:id) en vez de desplegar el texto ahí mismo; sin
 * `reviewId` cae en el comportamiento genérico de expandir/colapsar inline.
 */
@Component({
  selector: 'app-truncated-text',
  imports: [RouterLink],
  templateUrl: './truncated-text.html',
})
export class TruncatedText {
  text = input<string | null | undefined>(null);
  limit = input<number>(DEFAULT_LIMIT);
  reviewId = input<number | null | undefined>(null);

  protected isExpanded = signal(false);

  protected get isTruncated(): boolean {
    return (this.text()?.length ?? 0) > this.limit();
  }

  protected get visibleText(): string {
    const text = this.text() ?? '';
    if (this.isExpanded() || !this.isTruncated) return text;
    return text.slice(0, this.limit()) + '…';
  }

  toggleExpanded(): void {
    this.isExpanded.update((expanded) => !expanded);
  }
}
