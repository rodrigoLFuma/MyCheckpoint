import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserGameUserUserIdGet$Json } from '../../../api/fn/user-game/api-user-game-user-user-id-get-json';
import { apiUserGameIdDelete } from '../../../api/fn/user-game/api-user-game-id-delete';
import { apiGameListUserUserIdGet$Json } from '../../../api/fn/game-list/api-game-list-user-user-id-get-json';
import { apiGameListIdItemsPost } from '../../../api/fn/game-list/api-game-list-id-items-post';
import { UserGameDto } from '../../../api/models/user-game-dto';
import { GameListDto } from '../../../api/models/game-list-dto';
import { GameCard } from '../game-card/game-card';
import { TruncatedText } from '../../../shared/components/truncated-text/truncated-text';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';
import { Select, SelectOption } from '../../../shared/components/select/select';

// "Pendiente" no aparece aquí: una reseña ya creada implica que el juego
// dejó de ser "solo pendiente" (eso vive en la lista "Juegos pendientes").
const STATUSES = ['Jugando', 'Completado', 'Abandonado'] as const;

const SORT_OPTIONS = [
  { value: 'recent', label: 'Más recientes primero' },
  { value: 'oldest', label: 'Más antiguos primero' },
  { value: 'rate-desc', label: 'Mejor puntuados' },
  { value: 'rate-asc', label: 'Peor puntuados' },
  { value: 'name', label: 'Nombre (A-Z)' },
] as const;
type SortOption = (typeof SORT_OPTIONS)[number]['value'];

@Component({
  selector: 'app-library',
  imports: [FormsModule, RouterLink, GameCard, TruncatedText, Skeleton, Select],
  templateUrl: './library.html',
})
export class Library implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);

  protected readonly statuses = STATUSES;
  protected readonly sortOptions = SORT_OPTIONS;
  protected readonly statusFilterOptions: SelectOption[] = [
    { value: 'Todos', label: 'Todos los estados' },
    ...STATUSES.map((status) => ({ value: status, label: status })),
  ];
  protected userGames = signal<UserGameDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected searchTerm = signal('');
  protected statusFilter = signal<'Todos' | (typeof STATUSES)[number]>('Todos');
  protected sortBy = signal<SortOption>('recent');

  protected filteredGames = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const status = this.statusFilter();
    const sort = this.sortBy();

    let games = this.userGames();

    if (term) {
      games = games.filter(
        (g) => g.game.name.toLowerCase().includes(term) || (g.title ?? '').toLowerCase().includes(term),
      );
    }
    if (status !== 'Todos') {
      games = games.filter((g) => g.status === status);
    }

    games = [...games];
    switch (sort) {
      case 'oldest':
        games.sort((a, b) => a.createdAt.localeCompare(b.createdAt));
        break;
      case 'rate-desc':
        games.sort((a, b) => (b.rate ?? 0) - (a.rate ?? 0));
        break;
      case 'rate-asc':
        games.sort((a, b) => (a.rate ?? 0) - (b.rate ?? 0));
        break;
      case 'name':
        games.sort((a, b) => a.game.name.localeCompare(b.game.name));
        break;
      case 'recent':
      default:
        games.sort((a, b) => b.createdAt.localeCompare(a.createdAt));
        break;
    }

    return games;
  });

  protected pendingDeleteId = signal<number | null>(null);

  protected lists = signal<GameListDto[]>([]);
  protected listOptions = computed<SelectOption[]>(() =>
    this.lists().map((list) => ({ value: list.id, label: list.name })),
  );
  protected addingToListForGameId = signal<number | null>(null);
  protected addToListMessage = signal<string | null>(null);
  selectedListId: number | null = null;

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

  askDelete(id: number): void {
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
