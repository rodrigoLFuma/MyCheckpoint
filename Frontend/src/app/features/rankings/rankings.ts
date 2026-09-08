import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Api } from '../../api/api';
import { apiUserTopFollowedGet$Json } from '../../api/fn/user/api-user-top-followed-get-json';
import { apiGameTopRatedGet$Json } from '../../api/fn/game/api-game-top-rated-get-json';
import { apiUserGameTopLikedGet$Json } from '../../api/fn/user-game/api-user-game-top-liked-get-json';
import { RankedUserDto } from '../../api/models/ranked-user-dto';
import { TopRatedGameDto } from '../../api/models/top-rated-game-dto';
import { UserGameDto } from '../../api/models/user-game-dto';
import { UserAvatar } from '../../shared/components/user-avatar/user-avatar';
import { GameCard } from '../games/game-card/game-card';
import { ReviewCard } from '../../shared/components/review-card/review-card';
import { Skeleton } from '../../shared/components/skeleton/skeleton';

/**
 * Página pública de rankings: gente con más seguidores, reseñas con más
 * "me gusta" y juegos mejor valorados de toda la app. Las tres listas salen
 * de endpoints sin autenticación (para poder enseñarlas también desde la
 * landing), pero la propia ruta /rankings exige sesión igual que el resto
 * de botones del navbar.
 */
@Component({
  selector: 'app-rankings',
  imports: [RouterLink, DecimalPipe, UserAvatar, GameCard, ReviewCard, Skeleton],
  templateUrl: './rankings.html',
})
export class Rankings implements OnInit {
  private api = inject(Api);

  protected topUsers = signal<RankedUserDto[]>([]);
  protected isLoadingUsers = signal(true);

  protected topReviews = signal<UserGameDto[]>([]);
  protected isLoadingReviews = signal(true);

  protected topGames = signal<TopRatedGameDto[]>([]);
  protected isLoadingGames = signal(true);

  ngOnInit(): void {
    this.loadTopUsers();
    this.loadTopReviews();
    this.loadTopGames();
  }

  private async loadTopUsers(): Promise<void> {
    try {
      this.topUsers.set(await this.api.invoke(apiUserTopFollowedGet$Json, { take: 10 }));
    } catch {
      // Sin ranking de usuarios, esa sección simplemente queda vacía.
    } finally {
      this.isLoadingUsers.set(false);
    }
  }

  private async loadTopReviews(): Promise<void> {
    try {
      this.topReviews.set(await this.api.invoke(apiUserGameTopLikedGet$Json, { take: 10 }));
    } catch {
      // Sin ranking de reseñas, esa sección simplemente queda vacía.
    } finally {
      this.isLoadingReviews.set(false);
    }
  }

  private async loadTopGames(): Promise<void> {
    try {
      this.topGames.set(await this.api.invoke(apiGameTopRatedGet$Json, { take: 10 }));
    } catch {
      // Sin ranking de juegos, esa sección simplemente queda vacía.
    } finally {
      this.isLoadingGames.set(false);
    }
  }
}
