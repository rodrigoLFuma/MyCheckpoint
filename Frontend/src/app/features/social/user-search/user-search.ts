import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
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
  imports: [FormsModule, RouterLink, DatePipe, UserAvatar, Skeleton],
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

  // Recomendación por defecto, antes de que el usuario busque nada
  // explícitamente: un único listado (no agrupado por categoría) de gente
  // que comparte tus géneros favoritos, ordenado por cuántos compartís y
  // luego por número de seguidores (así lo hace GetSimilarUsersAsync en el
  // backend). Si no tienes géneros favoritos marcados, no hay nada "afín"
  // que recomendar todavía, así que se enseña en su lugar la gente con más
  // seguidores de toda la app.
  protected hasFavoriteGenres = computed(() => (this.authService.user()?.favoriteGenres?.length ?? 0) > 0);
  protected similarUsers = signal<UserSummaryDto[]>([]);
  protected topFollowed = signal<RankedUserDto[]>([]);
  protected isLoadingRecommended = signal(false);

  // Panel de notificaciones recientes junto al buscador: un adelanto, no
  // sustituye a /notifications (aquí no se marcan como leídas).
  protected notifications = signal<NotificationDto[]>([]);
  protected isLoadingNotifications = signal(true);

  // Mismo patrón para "Actividad": un adelanto de las últimas reseñas de la
  // gente que sigues, con enlace a /social/activity para ver todas.
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
      // Si falla, simplemente no se muestran recomendaciones.
    } finally {
      this.isLoadingRecommended.set(false);
    }
  }

  private async loadTopFollowed(): Promise<void> {
    this.isLoadingRecommended.set(true);
    try {
      this.topFollowed.set(await this.api.invoke(apiUserTopFollowedGet$Json, { take: RECOMMENDED_COUNT }));
    } catch {
      // Si falla, simplemente no se muestran recomendaciones.
    } finally {
      this.isLoadingRecommended.set(false);
    }
  }

  private async loadNotifications(): Promise<void> {
    this.isLoadingNotifications.set(true);
    try {
      this.notifications.set(await this.api.invoke(apiNotificationGet$Json, { skip: 0, take: NOTIFICATIONS_PREVIEW_COUNT }));
    } catch {
      // Si falla, el panel simplemente queda vacío.
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
      // Si falla, el panel simplemente queda vacío.
    } finally {
      this.isLoadingActivity.set(false);
    }
  }

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);
    this.isSearching.set(true);
    try {
      const found = await this.api.invoke(apiUserSearchGet$Json, { term: this.term.trim() || undefined });
      this.results.set(found);
      this.hasSearched.set(true);
    } catch {
      this.errorMessage.set('No se ha podido buscar gente. Inténtalo de nuevo.');
    } finally {
      this.isSearching.set(false);
    }
  }
}
