import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TruncatedText } from '../truncated-text/truncated-text';
import { LikeButton } from '../like-button/like-button';

/**
 * Tarjeta de una reseña: imagen y nombre del juego, título de la reseña,
 * descripción (recortada, vía TruncatedText), la nota y el botón de "me
 * gusta". Puramente visual, no sabe de dónde viene el UserGame.
 *
 * Toda la tarjeta (menos el "me gusta") es un único enlace a la reseña
 * completa (/reviews/:id): antes solo la imagen y el título llevaban ahí, y
 * el resto del texto no era clicable si no hacía falta un "Ver más" (reseñas
 * cortas o sin texto), así que a veces no pasaba nada al pulsar.
 */
@Component({
  selector: 'app-review-card',
  imports: [RouterLink, TruncatedText, LikeButton],
  templateUrl: './review-card.html',
})
export class ReviewCard {
  private authService = inject(AuthService);

  gameName = input.required<string>();
  gameImageUrl = input<string | null | undefined>(null);
  title = input<string | null | undefined>(null);
  description = input<string | null | undefined>(null);
  rate = input<number | null | undefined>(null);
  reviewId = input<number | null | undefined>(null);
  likeCount = input<number>(0);
  likedByMe = input<boolean>(false);

  // Para poder ofrecer "Añadir a mis reseñas" cuando la reseña es de otra
  // persona: hace falta el id local del juego (para /reviews/new/:gameId) y
  // el id de quien la escribió (para saber si eres tú).
  gameId = input<number | null | undefined>(null);
  ownerId = input<number | null | undefined>(null);

  protected isOwnReview = computed(() => {
    const ownerId = this.ownerId();
    return ownerId != null && ownerId === this.authService.user()?.id;
  });

  protected canAddToMyReviews = computed(
    () => this.authService.isLoggedIn() && !this.isOwnReview() && this.gameId() != null,
  );
}
