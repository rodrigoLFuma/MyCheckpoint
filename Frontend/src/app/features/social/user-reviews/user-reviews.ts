import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { apiUserIdGet$Json } from '../../../api/fn/user/api-user-id-get-json';
import { apiUserGameUserUserIdGet$Json } from '../../../api/fn/user-game/api-user-game-user-user-id-get-json';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { ReviewCard } from '../../../shared/components/review-card/review-card';

@Component({
  selector: 'app-user-reviews',
  imports: [RouterLink, ReviewCard],
  templateUrl: './user-reviews.html',
})
export class UserReviews implements OnInit {
  private api = inject(Api);
  private route = inject(ActivatedRoute);

  protected profileUser = signal<UserSummaryDto | null>(null);
  protected reviews = signal<UserGameDto[]>([]);
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
      const [user, reviews] = await Promise.all([
        this.api.invoke(apiUserIdGet$Json, { id: this.userId }),
        this.api.invoke(apiUserGameUserUserIdGet$Json, { userId: this.userId }),
      ]);
      this.profileUser.set(user);
      this.reviews.set(reviews);
    } catch {
      this.errorMessage.set('No se han podido cargar las reseñas.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
