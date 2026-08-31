import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserGameUserUserIdGet$Json } from '../../../api/fn/user-game/api-user-game-user-user-id-get-json';
import { apiUserGameIdPut$Json } from '../../../api/fn/user-game/api-user-game-id-put-json';
import { apiUserGameIdDelete } from '../../../api/fn/user-game/api-user-game-id-delete';
import { apiGameListUserUserIdGet$Json } from '../../../api/fn/game-list/api-game-list-user-user-id-get-json';
import { apiGameListIdItemsPost } from '../../../api/fn/game-list/api-game-list-id-items-post';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { GameListDto } from '../../../api/models/game-list-dto';
import { GameCard } from '../game-card/game-card';

// "Pendiente" no aparece aquí: una reseña ya creada implica que el juego
// dejó de ser "solo pendiente" (eso vive en la lista "Juegos pendientes").
const STATUSES = ['Jugando', 'Completado', 'Abandonado'] as const;

@Component({
  selector: 'app-library',
  imports: [FormsModule, RouterLink, GameCard],
  templateUrl: './library.html',
})
export class Library implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);

  protected readonly statuses = STATUSES;
  protected userGames = signal<UserGameDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected editingId = signal<number | null>(null);
  protected isSaving = signal(false);
  protected pendingDeleteId = signal<number | null>(null);

  protected lists = signal<GameListDto[]>([]);
  protected addingToListForGameId = signal<number | null>(null);
  protected addToListMessage = signal<string | null>(null);
  selectedListId: number | null = null;

  status: (typeof STATUSES)[number] = 'Jugando';
  rate = 5;
  title = '';
  review = '';

  ngOnInit(): void {
    this.loadLibrary();
    this.loadLists();
  }

  private async loadLists(): Promise<void> {
    const userId = this.authService.user()?.id;
    if (!userId) return;
    try {
      this.lists.set(await this.api.invoke(apiGameListUserUserIdGet$Json, { userId }));
    } catch {
      // Si no cargan las listas, simplemente no se ofrece la opción de añadir.
    }
  }

  openAddToList(gameId: number): void {
    this.editingId.set(null);
    this.pendingDeleteId.set(null);
    this.addToListMessage.set(null);
    this.selectedListId = this.lists()[0]?.id ?? null;
    this.addingToListForGameId.set(gameId);
  }

  cancelAddToList(): void {
    this.addingToListForGameId.set(null);
  }

  async confirmAddToList(gameId: number): Promise<void> {
    if (this.selectedListId === null) return;
    try {
      await this.api.invoke(apiGameListIdItemsPost, {
        id: this.selectedListId,
        body: { gameId },
      });
      this.addToListMessage.set('¡Añadido!');
      setTimeout(() => this.addingToListForGameId.set(null), 1000);
    } catch {
      this.addToListMessage.set('Ese juego ya está en esa lista, o no se ha podido añadir.');
    }
  }

  async loadLibrary(): Promise<void> {
    const userId = this.authService.user()?.id;
    if (!userId) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const games = await this.api.invoke(apiUserGameUserUserIdGet$Json, { userId });
      this.userGames.set(games);
    } catch {
      this.errorMessage.set('No se ha podido cargar tu biblioteca.');
    } finally {
      this.isLoading.set(false);
    }
  }

  startEdit(userGame: UserGameDto): void {
    this.pendingDeleteId.set(null);
    this.status = (userGame.status as (typeof STATUSES)[number]) ?? 'Jugando';
    this.rate = userGame.rate ?? 5;
    this.title = userGame.title ?? '';
    this.review = userGame.review ?? '';
    this.editingId.set(userGame.id);
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  async saveEdit(id: number): Promise<void> {
    this.isSaving.set(true);
    try {
      const updated = await this.api.invoke(apiUserGameIdPut$Json, {
        id,
        body: { status: this.status, rate: this.rate, title: this.title || null, review: this.review || null },
      });
      this.userGames.update((games) => games.map((g) => (g.id === id ? updated : g)));
      this.editingId.set(null);
    } catch {
      this.errorMessage.set('No se ha podido guardar el cambio.');
    } finally {
      this.isSaving.set(false);
    }
  }

  askDelete(id: number): void {
    this.editingId.set(null);
    this.pendingDeleteId.set(id);
  }

  cancelDelete(): void {
    this.pendingDeleteId.set(null);
  }

  async confirmDelete(id: number): Promise<void> {
    try {
      await this.api.invoke(apiUserGameIdDelete, { id });
      this.userGames.update((games) => games.filter((g) => g.id !== id));
    } catch {
      this.errorMessage.set('No se ha podido borrar la reseña.');
    } finally {
      this.pendingDeleteId.set(null);
    }
  }
}
