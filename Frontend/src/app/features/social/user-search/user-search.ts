import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe, NgClass } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserSearchGet$Json } from '../../../api/fn/user/api-user-search-get-json';
import { apiUserSimilarGet$Json } from '../../../api/fn/user/api-user-similar-get-json';
import { apiUserTopFollowedGet$Json } from '../../../api/fn/user/api-user-top-followed-get-json';
import { apiNotificationGet$Json } from '../../../api/fn/notification/api-notification-get-json';
import { apiUserGameFollowingActivityGet$Json } from '../../../api/fn/user-game/api-user-game-following-activity-get-json';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { RankedUserDto } from '../../../api/models/ranked-user-dto';
import { NotificationDto } from '../../../api/models/notification-dto';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

const RECOMMENDED_COUNT = 10;
const NOTIFICATIONS_PREVIEW_COUNT = 5;
const ACTIVITY_PREVIEW_COUNT = 3;

@Component({
  selector: 'app-user-search',
  imports: [FormsModule, RouterLink, DatePipe, NgClass, UserAvatar, Skeleton],
  templateUrl: './user-search.html',
})
export class UserSearch implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);

  term = '';
  results = signal<UserSummaryDto[]>([]);
  isSearching = signal(false);
  errorMessage = signal<string | null>(null);
  hasSearched = signal(false);

  protected hasFavoriteGenres = computed(() => (this.authService.user()?.favoriteGenres?.length ?? 0) > 0);
  protected similarUsers = signal<UserSummaryDto[]>([]);
  protected topFollowed = signal<RankedUserDto[]>([]);
  protected isLoadingRecommended = signal(false);

  protected recommendedExpanded = signal(false);

  protected notifications = signal<NotificationDto[]>([]);
  protected isLoadingNotifications = signal(true);

  protected activityPreview = signal<UserGameDto[]>([]);
  protected isLoadingActivity = signal(true);

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.hasFavoriteGenres() ? this.loadSimilarUsers() : this.loadTopFollowed();
      this.loadNotifications();
      this.loadActivityPreview();
    }
  }

  private async loadSimilarUsers(): Promise<void> {
    this.isLoadingRecommended.set(true);
    try {
      this.similarUsers.set(await this.api.invoke(apiUserSimilarGet$Json, { take: RECOMMENDED_COUNT }));
    } catch {

    } finally {
      this.isLoadingRecommended.set(false);
    }
  }

  private async loadTopFollowed(): Promise<void> {
    this.isLoadingRecommended.set(true);
    try {
      this.topFollowed.set(await this.api.invoke(apiUserTopFollowedGet$Json, { take: RECOMMENDED_COUNT }));
    } catch {

    } finally {
      this.isLoadingRecommended.set(false);
    }
  }

  private async loadNotifications(): Promise<void> {
    this.isLoadingNotifications.set(true);
    try {
      this.notifications.set(await this.api.invoke(apiNotificationGet$Json, { skip: 0, take: NOTIFICATIONS_PREVIEW_COUNT }));
    } catch {

    } finally {
      this.isLoadingNotifications.set(false);
    }
  }

  private async loadActivityPreview(): Promise<void> {
    this.isLoadingActivity.set(true);
    try {
      this.activityPreview.set(
        await this.api.invoke(apiUserGameFollowingActivityGet$Json, { skip: 0, take: ACTIVITY_PREVIEW_COUNT }),
      );
    } catch {

    } finally {
      this.isLoadingActivity.set(false);
    }
  }

  async onSubmit(): Promise<void> {
    const term = this.term.trim();

    if (!term) {
      this.hasSearched.set(false);
      return;
    }

    this.errorMessage.set(null);
    this.isSearching.set(true);
    try {
      const found = await this.api.invoke(apiUserSearchGet$Json, { term });
      this.results.set(found);
      this.hasSearched.set(true);
    } catch {
      this.errorMessage.set('No se ha podido buscar gente. Inténtalo de nuevo.');
    } finally {
      this.isSearching.set(false);
    }
  }
}
