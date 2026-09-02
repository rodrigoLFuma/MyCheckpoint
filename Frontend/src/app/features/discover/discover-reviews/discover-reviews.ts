import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { apiUserGameDiscoverReviewsGet$Json } from '../../../api/fn/user-game/api-user-game-discover-reviews-get-json';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { ReviewCard } from '../../../shared/components/review-card/review-card';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-discover-reviews',
  imports: [RouterLink, ReviewCard],
  templateUrl: './discover-reviews.html',
})
export class DiscoverReviews implements OnInit {
  private api = inject(Api);

  protected reviews = signal<UserGameDto[]>([]);
  protected isLoading = signal(true);
  protected isLoadingMore = signal(false);
  protected hasMore = signal(false);
  protected errorMessage = signal<string | null>(null);

  private loaded = 0;

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      const reviews = await this.api.invoke(apiUserGameDiscoverReviewsGet$Json, { skip: 0, take: PAGE_SIZE });
      this.reviews.set(reviews);
      this.loaded = reviews.length;
      this.hasMore.set(reviews.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se han podido cargar las reseñas.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async loadMore(): Promise<void> {
    this.isLoadingMore.set(true);
    try {
      const more = await this.api.invoke(apiUserGameDiscoverReviewsGet$Json, { skip: this.loaded, take: PAGE_SIZE });
      this.reviews.update((current) => [...current, ...more]);
      this.loaded += more.length;
      this.hasMore.set(more.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se han podido cargar más resultados.');
    } finally {
      this.isLoadingMore.set(false);
    }
  }
}
