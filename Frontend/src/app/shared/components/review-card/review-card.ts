import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TruncatedText } from '../truncated-text/truncated-text';
import { LikeButton } from '../like-button/like-button';

/**
 * Tarjeta de una reseña: imagen y nombre del juego, título de la reseña,
 * descripción (recortada con "Ver más" si hace falta, vía TruncatedText), la
 * nota y el botón de "me gusta" (vía LikeButton). Puramente visual, no sabe
 * de dónde viene el UserGame. La imagen y el título llevan a la reseña
 * completa (/reviews/:id).
 */
@Component({
  selector: 'app-review-card',
  imports: [RouterLink, TruncatedText, LikeButton],
  templateUrl: './review-card.html',
})
export class ReviewCard {
  gameName = input.required<string>();
  gameImageUrl = input<string | null | undefined>(null);
  title = input<string | null | undefined>(null);
  description = input<string | null | undefined>(null);
  rate = input<number | null | undefined>(null);
  reviewId = input<number | null | undefined>(null);
  likeCount = input<number>(0);
  likedByMe = input<boolean>(false);
}
