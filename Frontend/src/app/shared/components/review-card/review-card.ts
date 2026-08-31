import { Component, input, signal } from '@angular/core';

const DESCRIPTION_LIMIT = 400;

/**
 * Tarjeta de una reseña: imagen y nombre del juego, título de la reseña,
 * descripción (recortada a 400 caracteres con "Ver más" si hace falta) y
 * la nota. Puramente visual, no sabe de dónde viene el UserGame.
 */
@Component({
  selector: 'app-review-card',
  templateUrl: './review-card.html',
})
export class ReviewCard {
  gameName = input.required<string>();
  gameImageUrl = input<string | null | undefined>(null);
  title = input<string | null | undefined>(null);
  description = input<string | null | undefined>(null);
  rate = input<number | null | undefined>(null);

  protected isExpanded = signal(false);

  protected get isTruncated(): boolean {
    return (this.description()?.length ?? 0) > DESCRIPTION_LIMIT;
  }

  protected get visibleDescription(): string {
    const description = this.description() ?? '';
    if (this.isExpanded() || !this.isTruncated) return description;
    return description.slice(0, DESCRIPTION_LIMIT) + '…';
  }

  toggleExpanded(): void {
    this.isExpanded.update((expanded) => !expanded);
  }
}
