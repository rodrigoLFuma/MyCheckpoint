import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DatePipe, Location } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { apiUserGameIdGet$Json } from '../../../api/fn/user-game/api-user-game-id-get-json';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { AuthService } from '../../../core/services/auth.service';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';
import { LikeButton } from '../../../shared/components/like-button/like-button';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

@Component({
  selector: 'app-review-detail',
  imports: [DatePipe, RouterLink, UserAvatar, LikeButton, Skeleton],
  templateUrl: './review-detail.html',
})
export class ReviewDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(Api);
  private location = inject(Location);
  protected authService = inject(AuthService);

  protected userGame = signal<UserGameDto | null>(null);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected isOwnReview = computed(() => this.userGame()?.userSummary.id === this.authService.user()?.id);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.load(id);
  }

  private async load(id: number): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      this.userGame.set(await this.api.invoke(apiUserGameIdGet$Json, { id }));
    } catch {
      this.errorMessage.set('No se ha podido cargar la reseña.');
    } finally {
      this.isLoading.set(false);
    }
  }

  goBack(): void {
    this.location.back();
  }
}
