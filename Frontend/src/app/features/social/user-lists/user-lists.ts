import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserIdGet$Json } from '../../../api/fn/user/api-user-id-get-json';
import { apiGameListUserUserIdGet$Json } from '../../../api/fn/game-list/api-game-list-user-user-id-get-json';
import { apiGameListFollowedGet$Json } from '../../../api/fn/game-list/api-game-list-followed-get-json';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { GameListDto } from '../../../api/models/game-list-dto';
import { ListFollowButton } from '../../../shared/components/list-follow-button/list-follow-button';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

@Component({
  selector: 'app-user-lists',
  imports: [RouterLink, ListFollowButton, Skeleton],
  templateUrl: './user-lists.html',
})
export class UserLists implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);
  private route = inject(ActivatedRoute);

  protected profileUser = signal<UserSummaryDto | null>(null);
  protected lists = signal<GameListDto[]>([]);
  protected followedLists = signal<GameListDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);
  protected userId = 0;

  protected get isOwnProfile(): boolean {
    return this.userId === this.authService.user()?.id;
  }

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.userId = Number(this.route.snapshot.paramMap.get('id'));
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const [user, lists] = await Promise.all([
        this.api.invoke(apiUserIdGet$Json, { id: this.userId }),
        this.api.invoke(apiGameListUserUserIdGet$Json, { userId: this.userId }),
      ]);
      this.profileUser.set(user);
      this.lists.set(lists);

      if (this.isOwnProfile) {
        try {
          this.followedLists.set(await this.api.invoke(apiGameListFollowedGet$Json));
        } catch {

        }
      }
    } catch {
      this.errorMessage.set('No se han podido cargar las listas.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
