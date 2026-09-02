import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserIdGet$Json } from '../../../api/fn/user/api-user-id-get-json';
import { apiUserGameUserUserIdGet$Json } from '../../../api/fn/user-game/api-user-game-user-user-id-get-json';
import { apiGameListUserUserIdGet$Json } from '../../../api/fn/game-list/api-game-list-user-user-id-get-json';
import { apiFollowIsFollowingFollowedIdGet$Json } from '../../../api/fn/follow/api-follow-is-following-followed-id-get-json';
import { apiFollowPost } from '../../../api/fn/follow/api-follow-post';
import { apiFollowFollowedIdDelete } from '../../../api/fn/follow/api-follow-followed-id-delete';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { GameListDto } from '../../../api/models/game-list-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';
import { ReviewCard } from '../../../shared/components/review-card/review-card';

const PREVIEW_COUNT = 3;

@Component({
  selector: 'app-user-profile',
  imports: [RouterLink, UserAvatar, ReviewCard],
  templateUrl: './user-profile.html',
})
export class UserProfile implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);
  private route = inject(ActivatedRoute);

  protected profileUser = signal<UserSummaryDto | null>(null);
  protected reviews = signal<UserGameDto[]>([]);
  protected reviewCount = signal(0);
  protected lists = signal<GameListDto[]>([]);
  protected listCount = signal(0);
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
      const [user, reviews, lists] = await Promise.all([
        this.api.invoke(apiUserIdGet$Json, { id }),
        this.api.invoke(apiUserGameUserUserIdGet$Json, { userId: id }),
        this.api.invoke(apiGameListUserUserIdGet$Json, { userId: id }),
      ]);
      this.profileUser.set(user);
      this.reviewCount.set(reviews.length);
      this.reviews.set(reviews.slice(0, PREVIEW_COUNT));
      this.listCount.set(lists.length);
      this.lists.set(lists.slice(0, PREVIEW_COUNT));

      if (this.authService.isLoggedIn() && id !== this.authService.user()?.id) {
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
      } else {
        await this.api.invoke(apiFollowPost, { body: { followedId: user.id } });
        this.isFollowing.set(true);
      }
    } catch {
      this.errorMessage.set('No se ha podido actualizar el seguimiento.');
    } finally {
      this.isFollowBusy.set(false);
    }
  }
}
