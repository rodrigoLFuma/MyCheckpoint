import { Component, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiGameImportRawgIdPost$Json } from '../../../api/fn/game/api-game-import-rawg-id-post-json';

/**
 * Botón para reseñar un juego encontrado en RAWG. Solo importa el juego a
 * la BD local (import es idempotente: si ya existe por RawgId, no duplica,
 * solo lo devuelve) y navega a la pantalla completa de "nueva reseña"
 * (/reviews/new/:gameId), donde se rellena el formulario.
 */
@Component({
  selector: 'app-create-review',
  imports: [RouterLink],
  templateUrl: './create-review.html',
})
export class CreateReview {
  protected authService = inject(AuthService);
  private api = inject(Api);
  private router = inject(Router);

  rawgId = input.required<number>();
  alreadyReviewed = input<boolean>(false);

  protected isImporting = signal(false);
  protected errorMessage = signal<string | null>(null);

  async startReview(): Promise<void> {
    this.errorMessage.set(null);
    this.isImporting.set(true);
    try {
      const game = await this.api.invoke(apiGameImportRawgIdPost$Json, { rawgId: this.rawgId() });
      this.router.navigate(['/reviews/new', game.id]);
    } catch {
      this.errorMessage.set('No se ha podido importar el juego.');
    } finally {
      this.isImporting.set(false);
    }
  }
}
