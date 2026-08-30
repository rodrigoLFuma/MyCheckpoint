import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiGameImportRawgIdPost$Json } from '../../../api/fn/game/api-game-import-rawg-id-post-json';
import { apiUserGamePost$Json } from '../../../api/fn/user-game/api-user-game-post-json';
import { apiGameListPendingItemsPost } from '../../../api/fn/game-list/api-game-list-pending-items-post';

const STATUSES = ['Jugando', 'Completado', 'Abandonado', 'Pendiente'] as const;

/**
 * Botón + formulario para reseñar un juego encontrado en RAWG. Al enviar:
 * 1. Lo importa a la BD local (import es idempotente: si ya existe por
 *    RawgId, no duplica, solo lo devuelve).
 * 2. Crea el UserGame (reseña) asociado al usuario actual con ese GameId.
 */
@Component({
  selector: 'app-create-review',
  imports: [FormsModule, RouterLink],
  templateUrl: './create-review.html',
})
export class CreateReview {
  protected authService = inject(AuthService);
  private api = inject(Api);

  rawgId = input.required<number>();
  alreadyReviewed = input<boolean>(false);
  created = output<void>();

  protected readonly statuses = STATUSES;
  protected isFormOpen = signal(false);
  protected isSubmitting = signal(false);
  protected errorMessage = signal<string | null>(null);
  protected isDone = signal(false);

  status: (typeof STATUSES)[number] = 'Jugando';
  rate = 5;
  review = '';

  openForm(): void {
    this.errorMessage.set(null);
    this.isFormOpen.set(true);
  }

  cancel(): void {
    this.isFormOpen.set(false);
  }

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);
    this.isSubmitting.set(true);
    try {
      const game = await this.api.invoke(apiGameImportRawgIdPost$Json, { rawgId: this.rawgId() });

      if (this.status === 'Pendiente') {
        // "Pendiente" no es una reseña (no has jugado nada todavía): solo
        // apunta el juego en tu lista de backlog, sin crear un UserGame.
        await this.api.invoke(apiGameListPendingItemsPost, { body: { gameId: game.id } });
      } else {
        await this.api.invoke(apiUserGamePost$Json, {
          body: {
            gameId: game.id,
            status: this.status,
            rate: this.rate,
            review: this.review || null,
          },
        });
      }

      this.isFormOpen.set(false);
      this.isDone.set(true);
      this.created.emit();
    } catch {
      this.errorMessage.set('No se ha podido guardar.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
