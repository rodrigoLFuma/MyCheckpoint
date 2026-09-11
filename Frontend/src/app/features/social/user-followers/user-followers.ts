import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { apiUserIdGet$Json } from '../../../api/fn/user/api-user-id-get-json';
import { apiFollowFollowersUserIdGet$Json } from '../../../api/fn/follow/api-follow-followers-user-id-get-json';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

@Component({
  selector: 'app-user-followers',
  imports: [RouterLink, UserAvatar, Skeleton],
  templateUrl: './user-followers.html',
})
export class UserFollowers implements OnInit {
  private api = inject(Api);
  private route = inject(ActivatedRoute);

  protected profileUser = signal<UserSummaryDto | null>(null);
  protected followers = signal<UserSummaryDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);
  protected userId = 0;

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.userId = Number(this.route.snapshot.paramMap.get('id'));
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const [user, follows] = await Promise.all([
        this.api.invoke(apiUserIdGet$Json, { id: this.userId }),
        this.api.invoke(apiFollowFollowersUserIdGet$Json, { userId: this.userId }),
      ]);
      this.profileUser.set(user);

      this.followers.set(follows.map((f) => f.follower));
    } catch {
      this.errorMessage.set('No se han podido cargar los seguidores.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
