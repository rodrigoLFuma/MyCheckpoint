import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Api } from '../../api/api';
import { apiGameListUserUserIdGet$Json } from '../../api/fn/game-list/api-game-list-user-user-id-get-json';
import { apiGameListPost$Json } from '../../api/fn/game-list/api-game-list-post-json';
import { apiGameListFollowedGet$Json } from '../../api/fn/game-list/api-game-list-followed-get-json';
import { GameListDto } from '../../api/models/game-list-dto';
import { ListFollowButton } from '../../shared/components/list-follow-button/list-follow-button';
import { Skeleton } from '../../shared/components/skeleton/skeleton';

@Component({
  selector: 'app-lists',
  imports: [FormsModule, RouterLink, ListFollowButton, Skeleton],
  templateUrl: './lists.html',
})
export class Lists implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);

  protected lists = signal<GameListDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected followedLists = signal<GameListDto[]>([]);
  protected isLoadingFollowed = signal(true);

  protected isFormOpen = signal(false);
  protected isSubmitting = signal(false);
  protected formError = signal<string | null>(null);

  name = '';
  description = '';
  isPublic = true;

  ngOnInit(): void {
    this.loadLists();
    this.loadFollowedLists();
  }

  private async loadFollowedLists(): Promise<void> {
    this.isLoadingFollowed.set(true);
    try {
      this.followedLists.set(await this.api.invoke(apiGameListFollowedGet$Json));
    } catch {
      // Sin listas seguidas, la sección se queda vacía.
    } finally {
      this.isLoadingFollowed.set(false);
    }
  }

  async loadLists(): Promise<void> {
    const userId = this.authService.user()?.id;
    if (!userId) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const lists = await this.api.invoke(apiGameListUserUserIdGet$Json, { userId });
      this.lists.set(lists);
    } catch {
      this.errorMessage.set('No se han podido cargar tus listas.');
    } finally {
      this.isLoading.set(false);
    }
  }

  openForm(): void {
    this.name = '';
    this.description = '';
    this.isPublic = true;
    this.formError.set(null);
    this.isFormOpen.set(true);
  }

  cancelForm(): void {
    this.isFormOpen.set(false);
  }

  async onSubmit(): Promise<void> {
    this.formError.set(null);
    this.isSubmitting.set(true);
    try {
      const created = await this.api.invoke(apiGameListPost$Json, {
        body: { name: this.name, description: this.description || null, isPublic: this.isPublic },
      });
      this.lists.update((lists) => [created, ...lists]);
      this.isFormOpen.set(false);
    } catch (err) {
      const detail = err instanceof HttpErrorResponse ? err.error?.detail : null;
      this.formError.set(detail ?? 'No se ha podido crear la lista.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
