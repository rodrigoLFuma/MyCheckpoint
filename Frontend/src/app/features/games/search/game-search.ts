import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiGameSearchExternalGet$Json } from '../../../api/fn/game/api-game-search-external-get-json';
import { apiUserGameUserUserIdGet$Json } from '../../../api/fn/user-game/api-user-game-user-user-id-get-json';
import { apiCategoryGet$Json } from '../../../api/fn/category/api-category-get-json';
import { apiCategoryImportFromRawgPost$Json } from '../../../api/fn/category/api-category-import-from-rawg-post-json';
import { RawgGameSummary } from '../../../api/models/rawg-game-summary';
import { CategoryDto } from '../../../api/models/category-dto';
import { GameCard } from '../game-card/game-card';
import { CreateReview } from '../create-review/create-review';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

@Component({
  selector: 'app-game-search',
  imports: [FormsModule, GameCard, CreateReview, Skeleton],
  templateUrl: './game-search.html',
})
export class GameSearch implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);

  term = '';

  protected selectedCategoryIds = signal<Set<number>>(new Set());
  results = signal<RawgGameSummary[]>([]);
  isSearching = signal(false);
  isLoadingMore = signal(false);
  errorMessage = signal<string | null>(null);
  hasSearched = signal(false);
  hasMore = signal(false);

  private lastTerm: string | undefined;
  private lastGenres: string | undefined;
  private currentPage = 1;

  protected reviewedRawgIds = signal<Set<number>>(new Set());

  protected categories = signal<CategoryDto[]>([]);
  protected isSyncingCategories = signal(false);
  protected syncError = signal<string | null>(null);

  ngOnInit(): void {
    this.loadReviewedRawgIds();
    this.loadCategories();
  }

  private async loadCategories(): Promise<void> {
    try {
      this.categories.set(await this.api.invoke(apiCategoryGet$Json));
    } catch {

    }
  }

  async syncCategoriesFromRawg(): Promise<void> {
    this.isSyncingCategories.set(true);
    this.syncError.set(null);
    try {
      this.categories.set(await this.api.invoke(apiCategoryImportFromRawgPost$Json));
    } catch {
      this.syncError.set('No se han podido sincronizar las categorías.');
    } finally {
      this.isSyncingCategories.set(false);
    }
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

    }
  }

  toggleCategory(categoryId: number): void {
    this.selectedCategoryIds.update((ids) => {
      const next = new Set(ids);
      next.has(categoryId) ? next.delete(categoryId) : next.add(categoryId);
      return next;
    });
  }

  async onSubmit(): Promise<void> {
    const term = this.term.trim();
    const genres =
      this.categories()
        .filter((c) => this.selectedCategoryIds().has(c.id))
        .map((c) => c.rawgSlug)
        .filter((slug): slug is string => !!slug)
        .join(',') || undefined;
    if (!term && !genres) return;

    this.lastTerm = term || undefined;
    this.lastGenres = genres;
    this.currentPage = 1;

    this.errorMessage.set(null);
    this.isSearching.set(true);
    try {
      const found = await this.api.invoke(apiGameSearchExternalGet$Json, {
        term: this.lastTerm,
        genres: this.lastGenres,
        page: this.currentPage,
      });
      this.results.set(found.results ?? []);
      this.hasMore.set(found.hasMore ?? false);
      this.hasSearched.set(true);
    } catch {
      this.errorMessage.set('No se ha podido buscar en RAWG. Inténtalo de nuevo.');
    } finally {
      this.isSearching.set(false);
    }
  }

  async loadMore(): Promise<void> {
    this.isLoadingMore.set(true);
    try {
      const found = await this.api.invoke(apiGameSearchExternalGet$Json, {
        term: this.lastTerm,
        genres: this.lastGenres,
        page: this.currentPage + 1,
      });
      this.currentPage++;
      this.results.update((current) => [...current, ...(found.results ?? [])]);
      this.hasMore.set(found.hasMore ?? false);
    } catch {
      this.errorMessage.set('No se han podido cargar más resultados.');
    } finally {
      this.isLoadingMore.set(false);
    }
  }
}
