import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Api } from '../../api/api';
import { apiUserGameDiscoverReviewsGet$Json } from '../../api/fn/user-game/api-user-game-discover-reviews-get-json';
import { apiUserSimilarGet$Json } from '../../api/fn/user/api-user-similar-get-json';
import { apiGameSearchExternalGet$Json } from '../../api/fn/game/api-game-search-external-get-json';
import { apiUserGameUserUserIdGet$Json } from '../../api/fn/user-game/api-user-game-user-user-id-get-json';
import { UserGameDto } from '../../api/models/user-game-dto';
import { UserSummaryDto } from '../../api/models/user-summary-dto';
import { RawgGameSummary } from '../../api/models/rawg-game-summary';
import { GameCard } from '../games/game-card/game-card';
import { CreateReview } from '../games/create-review/create-review';
import { ReviewCard } from '../../shared/components/review-card/review-card';
import { UserAvatar } from '../../shared/components/user-avatar/user-avatar';

// Cuántas tarjetas se ven en la home antes de "Ver más" — el resto vive en
// la página completa de cada apartado.
const PREVIEW_COUNT = 6;
const DISCOVER_PICK_COUNT = 5;
const DISCOVER_RANDOM_PAGES = 5;

function shuffle<T>(items: T[]): T[] {
  const copy = [...items];
  for (let i = copy.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [copy[i], copy[j]] = [copy[j], copy[i]];
  }
  return copy;
}

@Component({
  selector: 'app-home',
  imports: [RouterLink, GameCard, CreateReview, ReviewCard, UserAvatar],
  templateUrl: './home.html',
})
export class Home implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);

  protected discoverReviews = signal<UserGameDto[]>([]);
  protected hasMoreReviews = signal(false);
  protected isLoadingReviews = signal(true);

  protected similarUsers = signal<UserSummaryDto[]>([]);
  protected hasMoreUsers = signal(false);
  protected isLoadingUsers = signal(true);

  // Popup de "Quiero descubrir mi nuevo juego favorito".
  protected isDiscoverPopupOpen = signal(false);
  protected isDiscovering = signal(false);
  protected discoverError = signal<string | null>(null);
  protected discoverPicks = signal<RawgGameSummary[]>([]);
  protected reviewedRawgIds = signal<Set<number>>(new Set());

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) return;
    this.loadDiscoverReviews();
    this.loadSimilarUsers();
    this.loadReviewedRawgIds();
  }

  private async loadDiscoverReviews(): Promise<void> {
    try {
      const reviews = await this.api.invoke(apiUserGameDiscoverReviewsGet$Json, { skip: 0, take: PREVIEW_COUNT });
      this.discoverReviews.set(reviews);
      this.hasMoreReviews.set(reviews.length === PREVIEW_COUNT);
    } catch {
      // Sin datos, la sección se queda vacía.
    } finally {
      this.isLoadingReviews.set(false);
    }
  }

  private async loadSimilarUsers(): Promise<void> {
    try {
      const users = await this.api.invoke(apiUserSimilarGet$Json, { skip: 0, take: PREVIEW_COUNT });
      this.similarUsers.set(users);
      this.hasMoreUsers.set(users.length === PREVIEW_COUNT);
    } catch {
      // Sin datos, la sección se queda vacía.
    } finally {
      this.isLoadingUsers.set(false);
    }
  }

  private async loadReviewedRawgIds(): Promise<void> {
    const userId = this.authService.user()?.id;
    if (!userId) return;
    try {
      const userGames = await this.api.invoke(apiUserGameUserUserIdGet$Json, { userId });
      const rawgIds = userGames.map((ug) => ug.game.rawgId).filter((id): id is number => id != null);
      this.reviewedRawgIds.set(new Set(rawgIds));
    } catch {
      // Si no cargan, simplemente no se marca ningún resultado como reseñado.
    }
  }

  onDiscoverReviewCreated(rawgId: number): void {
    this.reviewedRawgIds.update((ids) => new Set(ids).add(rawgId));
  }

  async discoverNewFavorite(): Promise<void> {
    const genres = (this.authService.user()?.favoriteGenres ?? [])
      .map((c) => c.rawgSlug)
      .filter((slug): slug is string => !!slug)
      .join(',');
    if (!genres) return;

    this.isDiscovering.set(true);
    this.discoverError.set(null);
    this.discoverPicks.set([]);
    this.isDiscoverPopupOpen.set(true);
    try {
      // Página al azar para que cada pulsación dé variedad, en vez de
      // enseñar siempre los mismos resultados "top" de tus géneros.
      const page = Math.floor(Math.random() * DISCOVER_RANDOM_PAGES) + 1;
      const found = await this.api.invoke(apiGameSearchExternalGet$Json, { genres, page });
      this.discoverPicks.set(shuffle(found.results ?? []).slice(0, DISCOVER_PICK_COUNT));
    } catch {
      this.discoverError.set('No se ha podido conectar con RAWG. Inténtalo de nuevo.');
    } finally {
      this.isDiscovering.set(false);
    }
  }

  closeDiscoverPopup(): void {
    this.isDiscoverPopupOpen.set(false);
  }
}
