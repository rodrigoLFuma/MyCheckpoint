import { Component, inject, input, signal } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiGameListIdFollowPost } from '../../../api/fn/game-list/api-game-list-id-follow-post';
import { apiGameListIdFollowDelete } from '../../../api/fn/game-list/api-game-list-id-follow-delete';

/**
 * Botón de "Seguir"/"Siguiendo" una lista pública, con el nº de seguidores.
 * Habla con la API por su cuenta (solo necesita el id de la lista), con
 * estado optimista local ya que los endpoints de seguir/dejar de seguir no
 * devuelven el recuento actualizado.
 */
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
    } catch {
      // Si falla, se queda como estaba: no hay nada más que mostrar aquí.
    } finally {
      this.isToggling.set(false);
    }
  }
}
