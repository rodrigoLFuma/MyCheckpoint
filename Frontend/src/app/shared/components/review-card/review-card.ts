import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TruncatedText } from '../truncated-text/truncated-text';
import { LikeButton } from '../like-button/like-button';

@Component({
  selector: 'app-review-card',
  imports: [RouterLink, TruncatedText, LikeButton],
  templateUrl: './review-card.html',
})
export class ReviewCard {
  private authService = inject(AuthService);

  gameName = input.required<string>();
  gameImageUrl = input<string | null | undefined>(null);
  title = input<string | null | undefined>(null);
  description = input<string | null | undefined>(null);
  rate = input<number | null | undefined>(null);
  reviewId = input<number | null | undefined>(null);
  likeCount = input<number>(0);
  likedByMe = input<boolean>(false);

  gameId = input<number | null | undefined>(null);
  ownerId = input<number | null | undefined>(null);

  protected isOwnReview = computed(() => {
    const ownerId = this.ownerId();
    return ownerId != null && ownerId === this.authService.user()?.id;
  });

  protected canAddToMyReviews = computed(
    () => this.authService.isLoggedIn() && !this.isOwnReview() && this.gameId() != null,
  );
}
