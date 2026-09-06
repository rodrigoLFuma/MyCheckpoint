import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { apiGameListRecommendedGet$Json } from '../../../api/fn/game-list/api-game-list-recommended-get-json';
import { GameListDto } from '../../../api/models/game-list-dto';
import { ListFollowButton } from '../../../shared/components/list-follow-button/list-follow-button';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

const PAGE_SIZE = 10;

/**
 * Listas públicas ajenas que contienen algún juego que ya tienes, ordenadas
 * por nº de seguidores — la recomendación es "juegos en común + popularidad",
 * no una búsqueda libre de listas.
 */
@Component({
  selector: 'app-discover-lists',
  imports: [RouterLink, ListFollowButton, Skeleton],
  templateUrl: './discover-lists.html',
})
export class DiscoverLists implements OnInit {
  private api = inject(Api);

  protected lists = signal<GameListDto[]>([]);
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
    this.errorMessage.set(null);
    try {
      const lists = await this.api.invoke(apiGameListRecommendedGet$Json, { skip: 0, take: PAGE_SIZE });
      this.lists.set(lists);
      this.loaded = lists.length;
      this.hasMore.set(lists.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se han podido cargar las listas recomendadas.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async loadMore(): Promise<void> {
    this.isLoadingMore.set(true);
    try {
      const more = await this.api.invoke(apiGameListRecommendedGet$Json, { skip: this.loaded, take: PAGE_SIZE });
      this.lists.update((current) => [...current, ...more]);
      this.loaded += more.length;
      this.hasMore.set(more.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se han podido cargar más listas.');
    } finally {
      this.isLoadingMore.set(false);
    }
  }
}
