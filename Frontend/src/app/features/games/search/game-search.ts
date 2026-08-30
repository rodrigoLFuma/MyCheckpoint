import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiGameSearchExternalGet$Json } from '../../../api/fn/game/api-game-search-external-get-json';
import { apiUserGameUserUserIdGet$Json } from '../../../api/fn/user-game/api-user-game-user-user-id-get-json';
import { RawgGameSummary } from '../../../api/models/rawg-game-summary';
import { GameCard } from '../game-card/game-card';
import { CreateReview } from '../create-review/create-review';

@Component({
  selector: 'app-game-search',
  imports: [FormsModule, GameCard, CreateReview],
  templateUrl: './game-search.html',
})
export class GameSearch implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);

  term = '';
  results = signal<RawgGameSummary[]>([]);
  isSearching = signal(false);
  errorMessage = signal<string | null>(null);
  hasSearched = signal(false);

  // RawgId de los juegos que el usuario ya tiene reseñados, para no
  // ofrecerle "Crear reseña" otra vez en los resultados de búsqueda.
  protected reviewedRawgIds = signal<Set<number>>(new Set());

  ngOnInit(): void {
    this.loadReviewedRawgIds();
  }

  private async loadReviewedRawgIds(): Promise<void> {
    const userId = this.authService.user()?.id;
    if (!userId) return;
    try {
      const userGames = await this.api.invoke(apiUserGameUserUserIdGet$Json, { userId });
      const rawgIds = userGames
        .map((ug) => ug.game.rawgId)
        .filter((id): id is number => id != null);
      this.reviewedRawgIds.set(new Set(rawgIds));
    } catch {
      // Si no cargan, simplemente no se marca ningún resultado como reseñado.
    }
  }

  onReviewCreated(rawgId: number): void {
    this.reviewedRawgIds.update((ids) => new Set(ids).add(rawgId));
  }

  async onSubmit(): Promise<void> {
    const term = this.term.trim();
    if (!term) return;

    this.errorMessage.set(null);
    this.isSearching.set(true);
    try {
      const found = await this.api.invoke(apiGameSearchExternalGet$Json, { term });
      this.results.set(found);
      this.hasSearched.set(true);
    } catch {
      this.errorMessage.set('No se ha podido buscar en RAWG. Inténtalo de nuevo.');
    } finally {
      this.isSearching.set(false);
    }
  }
}
