import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Api } from '../../api/api';
import { apiGameTopRatedGet$Json } from '../../api/fn/game/api-game-top-rated-get-json';
import { apiUserGameTopLikedGet$Json } from '../../api/fn/user-game/api-user-game-top-liked-get-json';
import { UserGameDto } from '../../api/models/user-game-dto';
import { TopRatedGameDto } from '../../api/models/top-rated-game-dto';
import { GameCard } from '../games/game-card/game-card';

/**
 * Home pública: la misma landing para todo el mundo, con sesión iniciada o
 * sin ella (antes había un panel personal aparte para usuarios logueados;
 * se quitó para que "/" sea siempre esta única vista). Muestra los juegos
 * mejor valorados y una reseña destacada con datos reales de la app; los
 * botones llevan a rutas que exigen sesión, así que sin ella el propio
 * guard te manda a /login y te trae de vuelta a donde ibas.
 */
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
      this.landingTopGames.set(await this.api.invoke(apiGameTopRatedGet$Json, { take: 10 }));
    } catch {
      // Sin juegos puntuados todavía, el carrusel simplemente no aparece.
    }
  }

  private async loadFeaturedReview(): Promise<void> {
    try {
      const [top] = await this.api.invoke(apiUserGameTopLikedGet$Json, { take: 1 });
      this.featuredReview.set(top ?? null);
    } catch {
      // Sin reseñas destacadas todavía, esa sección simplemente no aparece.
    }
  }
}
