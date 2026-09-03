import { Component, inject, input, signal } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserGameIdLikePost$Json } from '../../../api/fn/user-game/api-user-game-id-like-post-json';
import { apiUserGameIdLikeDelete$Json } from '../../../api/fn/user-game/api-user-game-id-like-delete-json';

/**
 * Botón de "me gusta" de una reseña. Habla con la API por su cuenta (solo
 * necesita el reviewId), con estado optimista local: al pulsar, manda lo que
 * devuelve la API sobre lo que venga por input, para no depender de que el
 * padre recargue la lista entera. Sin sesión iniciada se muestra el conteo
 * pero no es interactivo.
 */
@Component({
  selector: 'app-like-button',
  templateUrl: './like-button.html',
})
export class LikeButton {
  private authService = inject(AuthService);
  private api = inject(Api);

  reviewId = input.required<number>();
  likeCount = input<number>(0);
  likedByMe = input<boolean>(false);

  private localLikeCount = signal<number | null>(null);
  private localLikedByMe = signal<boolean | null>(null);
  protected isToggling = signal(false);

  protected get displayedLikeCount(): number {
    return this.localLikeCount() ?? this.likeCount();
  }

  protected get displayedLikedByMe(): boolean {
    return this.localLikedByMe() ?? this.likedByMe();
  }

  protected get canLike(): boolean {
    return this.authService.isLoggedIn();
  }

  async toggle(): Promise<void> {
    if (!this.canLike || this.isToggling()) return;

    this.isToggling.set(true);
    try {
      const id = this.reviewId();
      const updated = this.displayedLikedByMe
        ? await this.api.invoke(apiUserGameIdLikeDelete$Json, { id })
        : await this.api.invoke(apiUserGameIdLikePost$Json, { id });
      this.localLikeCount.set(updated.likeCount);
      this.localLikedByMe.set(updated.likedByMe);
    } catch {
      // Si falla, se queda como estaba: no hay nada más que mostrar aquí.
    } finally {
      this.isToggling.set(false);
    }
  }
}
