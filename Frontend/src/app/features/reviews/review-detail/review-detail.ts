import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe, Location } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { apiUserGameIdGet$Json } from '../../../api/fn/user-game/api-user-game-id-get-json';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';

/**
 * Vista completa de una reseña: a donde lleva "Ver más" en cualquier tarjeta
 * de reseña (ReviewCard o Mi biblioteca), en vez de desplegar el texto ahí
 * mismo. Pública (como la vista de un perfil), no solo para tus reseñas.
 */
@Component({
  selector: 'app-review-detail',
  imports: [DatePipe, RouterLink, UserAvatar],
  templateUrl: './review-detail.html',
})
export class ReviewDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(Api);
  private location = inject(Location);

  protected userGame = signal<UserGameDto | null>(null);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

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
