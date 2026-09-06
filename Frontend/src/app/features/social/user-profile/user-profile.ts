import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserIdGet$Json } from '../../../api/fn/user/api-user-id-get-json';
import { apiUserIdStatsGet$Json } from '../../../api/fn/user/api-user-id-stats-get-json';
import { apiUserGameUserUserIdGet$Json } from '../../../api/fn/user-game/api-user-game-user-user-id-get-json';
import { apiGameListUserUserIdGet$Json } from '../../../api/fn/game-list/api-game-list-user-user-id-get-json';
import { apiGameListFollowedGet$Json } from '../../../api/fn/game-list/api-game-list-followed-get-json';
import { apiFollowIsFollowingFollowedIdGet$Json } from '../../../api/fn/follow/api-follow-is-following-followed-id-get-json';
import { apiFollowFollowersUserIdGet$Json } from '../../../api/fn/follow/api-follow-followers-user-id-get-json';
import { apiFollowFollowingUserIdGet$Json } from '../../../api/fn/follow/api-follow-following-user-id-get-json';
import { apiFollowPost } from '../../../api/fn/follow/api-follow-post';
import { apiFollowFollowedIdDelete } from '../../../api/fn/follow/api-follow-followed-id-delete';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { UserStatsDto } from '../../../api/models/user-stats-dto';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { GameListDto } from '../../../api/models/game-list-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';
import { ReviewCard } from '../../../shared/components/review-card/review-card';
import { ListFollowButton } from '../../../shared/components/list-follow-button/list-follow-button';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

const PREVIEW_COUNT = 3;

@Component({
  selector: 'app-user-profile',
  imports: [RouterLink, UserAvatar, ReviewCard, ListFollowButton, Skeleton, DecimalPipe],
  templateUrl: './user-profile.html',
})
export class UserProfile implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);
  private route = inject(ActivatedRoute);

  protected profileUser = signal<UserSummaryDto | null>(null);
  protected stats = signal<UserStatsDto | null>(null);
  protected reviews = signal<UserGameDto[]>([]);
  protected reviewCount = signal(0);
  protected lists = signal<GameListDto[]>([]);
  protected listCount = signal(0);
  protected followedLists = signal<GameListDto[]>([]);
  protected followedListCount = signal(0);
  protected followerCount = signal(0);
  protected followingCount = signal(0);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected isFollowing = signal(false);
  protected isFollowBusy = signal(false);

  protected get isOwnProfile(): boolean {
    return this.profileUser()?.id === this.authService.user()?.id;
  }

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const isOwn = id === this.authService.user()?.id;

      const [user, stats, reviews, lists, followers, following] = await Promise.all([
        this.api.invoke(apiUserIdGet$Json, { id }),
        this.api.invoke(apiUserIdStatsGet$Json, { id }),
        this.api.invoke(apiUserGameUserUserIdGet$Json, { userId: id }),
        this.api.invoke(apiGameListUserUserIdGet$Json, { userId: id }),
        this.api.invoke(apiFollowFollowersUserIdGet$Json, { userId: id }),
        this.api.invoke(apiFollowFollowingUserIdGet$Json, { userId: id }),
      ]);
      this.profileUser.set(user);
      this.stats.set(stats);
      this.reviewCount.set(reviews.length);
      this.reviews.set(reviews.slice(0, PREVIEW_COUNT));
      this.listCount.set(lists.length);
      this.lists.set(lists.slice(0, PREVIEW_COUNT));
      this.followerCount.set(followers.length);
      this.followingCount.set(following.length);

      // Las listas que sigues (de otros) solo se ven en tu propio perfil,
      // nunca en el de nadie más te busque a ti.
      if (isOwn) {
        try {
          const followedLists = await this.api.invoke(apiGameListFollowedGet$Json);
          this.followedListCount.set(followedLists.length);
          this.followedLists.set(followedLists.slice(0, PREVIEW_COUNT));
        } catch {
          // Sin listas seguidas, la sección se queda vacía.
        }
      }

      if (this.authService.isLoggedIn() && !isOwn) {
        try {
          this.isFollowing.set(await this.api.invoke(apiFollowIsFollowingFollowedIdGet$Json, { followedId: id }));
        } catch {
          // Si falla la comprobación, se queda como "no siguiendo".
        }
      }
    } catch {
      this.errorMessage.set('Este usuario no existe.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async toggleFollow(): Promise<void> {
    const user = this.profileUser();
    if (!user) return;

    this.isFollowBusy.set(true);
    try {
      if (this.isFollowing()) {
        await this.api.invoke(apiFollowFollowedIdDelete, { followedId: user.id });
        this.isFollowing.set(false);
        this.followerCount.update((count) => count - 1);
      } else {
        await this.api.invoke(apiFollowPost, { body: { followedId: user.id } });
        this.isFollowing.set(true);
        this.followerCount.update((count) => count + 1);
      }
    } catch {
      this.errorMessage.set('No se ha podido actualizar el seguimiento.');
    } finally {
      this.isFollowBusy.set(false);
    }
  }
}
