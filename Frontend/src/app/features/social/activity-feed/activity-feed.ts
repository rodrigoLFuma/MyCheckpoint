import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserGameFollowingActivityGet$Json } from '../../../api/fn/user-game/api-user-game-following-activity-get-json';
import { apiFollowFollowingUserIdGet$Json } from '../../../api/fn/follow/api-follow-following-user-id-get-json';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { ReviewCard } from '../../../shared/components/review-card/review-card';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-activity-feed',
  imports: [RouterLink, ReviewCard, UserAvatar, Skeleton],
  templateUrl: './activity-feed.html',
})
export class ActivityFeed implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);

  protected activity = signal<UserGameDto[]>([]);
  protected isLoading = signal(true);
  protected isLoadingMore = signal(false);
  protected hasMore = signal(false);
  protected errorMessage = signal<string | null>(null);

  protected followsAnyone = signal(true);

  private loaded = 0;

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const userId = this.authService.user()?.id;
      if (userId) {
        const following = await this.api.invoke(apiFollowFollowingUserIdGet$Json, { userId });
        this.followsAnyone.set(following.length > 0);
      }

      const activity = await this.api.invoke(apiUserGameFollowingActivityGet$Json, { skip: 0, take: PAGE_SIZE });
      this.activity.set(activity);
      this.loaded = activity.length;
      this.hasMore.set(activity.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se ha podido cargar la actividad.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async loadMore(): Promise<void> {
    this.isLoadingMore.set(true);
    try {
      const more = await this.api.invoke(apiUserGameFollowingActivityGet$Json, { skip: this.loaded, take: PAGE_SIZE });
      this.activity.update((current) => [...current, ...more]);
      this.loaded += more.length;
      this.hasMore.set(more.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se han podido cargar más resultados.');
    } finally {
      this.isLoadingMore.set(false);
    }
  }
}
