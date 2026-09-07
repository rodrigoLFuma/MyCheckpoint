import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiGameListIdGet$Json } from '../../../api/fn/game-list/api-game-list-id-get-json';
import { apiGameListIdPut$Json } from '../../../api/fn/game-list/api-game-list-id-put-json';
import { apiGameListIdDelete } from '../../../api/fn/game-list/api-game-list-id-delete';
import { apiGameListIdItemsGameIdDelete } from '../../../api/fn/game-list/api-game-list-id-items-game-id-delete';
import { GameListDto } from '../../../api/models/game-list-dto';
import { GameCard } from '../../games/game-card/game-card';
import { ListFollowButton } from '../../../shared/components/list-follow-button/list-follow-button';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

@Component({
  selector: 'app-list-detail',
  imports: [FormsModule, GameCard, ListFollowButton, Skeleton],
  templateUrl: './list-detail.html',
})
export class ListDetail implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  protected list = signal<GameListDto | null>(null);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected isEditing = signal(false);
  protected isSaving = signal(false);
  protected formError = signal<string | null>(null);
  protected pendingDelete = signal(false);

  name = '';
  description = '';
  isPublic = true;

  protected get isOwner(): boolean {
    const list = this.list();
    return !!list && list.user.id === this.authService.user()?.id;
  }

  ngOnInit(): void {
    this.load();
  }

  async load(): Promise<void> {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const list = await this.api.invoke(apiGameListIdGet$Json, { id });
      this.list.set(list);
    } catch (err) {
      this.errorMessage.set(
        err instanceof HttpErrorResponse && err.status === 403
          ? 'Esta lista es privada.'
          : 'Esta lista no existe.',
      );
    } finally {
      this.isLoading.set(false);
    }
  }

  startEdit(): void {
    const list = this.list();
    if (!list) return;
    this.name = list.name;
    this.description = list.description ?? '';
    this.isPublic = list.isPublic;
    this.formError.set(null);
    this.isEditing.set(true);
  }

  cancelEdit(): void {
    this.isEditing.set(false);
  }

  async saveEdit(): Promise<void> {
    const list = this.list();
    if (!list) return;

    this.isSaving.set(true);
    this.formError.set(null);
    try {
      const updated = await this.api.invoke(apiGameListIdPut$Json, {
        id: list.id,
        body: { name: this.name, description: this.description || null, isPublic: this.isPublic },
      });
      this.list.set(updated);
      this.isEditing.set(false);
    } catch (err) {
      const detail = err instanceof HttpErrorResponse ? err.error?.detail : null;
      this.formError.set(detail ?? 'No se ha podido guardar la lista.');
    } finally {
      this.isSaving.set(false);
    }
  }

  askDelete(): void {
    this.pendingDelete.set(true);
  }

  cancelDelete(): void {
    this.pendingDelete.set(false);
  }

  async confirmDelete(): Promise<void> {
    const list = this.list();
    if (!list) return;
    await this.api.invoke(apiGameListIdDelete, { id: list.id });
    this.router.navigateByUrl('/lists');
  }

  onFollowerCountChange(count: number): void {
    this.list.update((current) => (current ? { ...current, followerCount: count } : current));
  }

  async removeGame(gameId: number): Promise<void> {
    const list = this.list();
    if (!list) return;
    await this.api.invoke(apiGameListIdItemsGameIdDelete, { id: list.id, gameId });
    this.list.update((current) =>
      current ? { ...current, games: current.games?.filter((g) => g.game.id !== gameId) } : current,
    );
  }
}
