import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Api } from '../../api/api';
import { apiGameTopRatedGet$Json } from '../../api/fn/game/api-game-top-rated-get-json';
import { apiUserGameTopLikedGet$Json } from '../../api/fn/user-game/api-user-game-top-liked-get-json';
import { UserGameDto } from '../../api/models/user-game-dto';
import { TopRatedGameDto } from '../../api/models/top-rated-game-dto';
import { GameCard } from '../games/game-card/game-card';

@Component({
  selector: 'app-home',
  imports: [RouterLink, GameCard],
  templateUrl: './home.html',
})
export class Home implements OnInit {
  private api = inject(Api);

  protected landingTopGames = signal<TopRatedGameDto[]>([]);
  protected featuredReview = signal<UserGameDto | null>(null);

  ngOnInit(): void {
    this.loadLandingTopGames();
    this.loadFeaturedReview();
  }

  private async loadLandingTopGames(): Promise<void> {
    try {
      this.landingTopGames.set(await this.api.invoke(apiGameTopRatedGet$Json, { take: 5 }));
    } catch {

    }
  }

  private async loadFeaturedReview(): Promise<void> {
    try {
      const [top] = await this.api.invoke(apiUserGameTopLikedGet$Json, { take: 1 });
      this.featuredReview.set(top ?? null);
    } catch {

    }
  }
}
