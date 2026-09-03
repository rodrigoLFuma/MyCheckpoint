import { Component, input } from '@angular/core';
import { TruncatedText } from '../truncated-text/truncated-text';

/**
 * Tarjeta de una reseña: imagen y nombre del juego, título de la reseña,
 * descripción (recortada con "Ver más" si hace falta, vía TruncatedText) y
 * la nota. Puramente visual, no sabe de dónde viene el UserGame.
 */
@Component({
  selector: 'app-review-card',
  imports: [TruncatedText],
  templateUrl: './review-card.html',
})
export class ReviewCard {
  gameName = input.required<string>();
  gameImageUrl = input<string | null | undefined>(null);
  title = input<string | null | undefined>(null);
  description = input<string | null | undefined>(null);
  rate = input<number | null | undefined>(null);
  reviewId = input<number | null | undefined>(null);
}
