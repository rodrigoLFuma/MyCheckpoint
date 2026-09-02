import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { apiUserSimilarGet$Json } from '../../../api/fn/user/api-user-similar-get-json';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';

const PAGE_SIZE = 24;

@Component({
  selector: 'app-discover-users',
  imports: [RouterLink, UserAvatar],
  templateUrl: './discover-users.html',
})
export class DiscoverUsers implements OnInit {
  private api = inject(Api);

  protected users = signal<UserSummaryDto[]>([]);
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
      const users = await this.api.invoke(apiUserSimilarGet$Json, { skip: 0, take: PAGE_SIZE });
      this.users.set(users);
      this.loaded = users.length;
      this.hasMore.set(users.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se ha podido cargar la lista.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async loadMore(): Promise<void> {
    this.isLoadingMore.set(true);
    try {
      const more = await this.api.invoke(apiUserSimilarGet$Json, { skip: this.loaded, take: PAGE_SIZE });
      this.users.update((current) => [...current, ...more]);
      this.loaded += more.length;
      this.hasMore.set(more.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se han podido cargar más resultados.');
    } finally {
      this.isLoadingMore.set(false);
    }
  }
}
