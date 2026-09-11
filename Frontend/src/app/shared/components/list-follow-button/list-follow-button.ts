import { Component, inject, input, output, signal } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiGameListIdFollowPost } from '../../../api/fn/game-list/api-game-list-id-follow-post';
import { apiGameListIdFollowDelete } from '../../../api/fn/game-list/api-game-list-id-follow-delete';

@Component({
  selector: 'app-list-follow-button',
  templateUrl: './list-follow-button.html',
})
export class ListFollowButton {
  private authService = inject(AuthService);
  private api = inject(Api);

  gameListId = input.required<number>();
  followerCount = input<number>(0);
  followedByMe = input<boolean>(false);
  isOwner = input<boolean>(false);

  followerCountChange = output<number>();

  private localCount = signal<number | null>(null);
  private localFollowed = signal<boolean | null>(null);
  protected isToggling = signal(false);

  protected get displayedCount(): number {
    return this.localCount() ?? this.followerCount();
  }

  protected get displayedFollowed(): boolean {
    return this.localFollowed() ?? this.followedByMe();
  }

  protected get canFollow(): boolean {
    return this.authService.isLoggedIn() && !this.isOwner();
  }

  async toggle(): Promise<void> {
    if (!this.canFollow || this.isToggling()) return;

    this.isToggling.set(true);
    try {
      const id = this.gameListId();
      if (this.displayedFollowed) {
        await this.api.invoke(apiGameListIdFollowDelete, { id });
        this.localFollowed.set(false);
        this.localCount.set(this.displayedCount - 1);
      } else {
        await this.api.invoke(apiGameListIdFollowPost, { id });
        this.localFollowed.set(true);
        this.localCount.set(this.displayedCount + 1);
      }
      this.followerCountChange.emit(this.displayedCount);
    } catch {

    } finally {
      this.isToggling.set(false);
    }
  }
}
