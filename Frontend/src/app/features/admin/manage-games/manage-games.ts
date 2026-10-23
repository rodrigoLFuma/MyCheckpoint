import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../../../api/api';
import { apiGameGet$Json } from '../../../api/fn/game/api-game-get-json';
import { apiGamePost$Json } from '../../../api/fn/game/api-game-post-json';
import { apiGameIdPut$Json } from '../../../api/fn/game/api-game-id-put-json';
import { apiGameIdDelete } from '../../../api/fn/game/api-game-id-delete';
import { apiGameRefreshPost$Json } from '../../../api/fn/game/api-game-refresh-post-json';
import { apiCategoryGet$Json } from '../../../api/fn/category/api-category-get-json';
import { CategoryDto } from '../../../api/models/category-dto';
import { GameDto } from '../../../api/models/game-dto';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';
import { extractErrorDetail } from '../../../shared/utils/http-error';

interface GameForm {
  name: string;
  releaseDate: string;
  developer: string;
  publisher: string;
  description: string;
  coverImageUrl: string;
  metacritic: number | null;
}

const emptyForm = (): GameForm => ({
  name: '',
  releaseDate: '',
  developer: '',
  publisher: '',
  description: '',
  coverImageUrl: '',
  metacritic: null,
});

@Component({
  selector: 'app-manage-games',
  imports: [DatePipe, FormsModule, Skeleton],
  templateUrl: './manage-games.html',
})
export class ManageGames implements OnInit {
  private api = inject(Api);

  protected games = signal<GameDto[]>([]);
  protected categories = signal<CategoryDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected selectedIds = signal<Set<number>>(new Set());
  protected isRefreshing = signal(false);
  protected refreshMessage = signal<string | null>(null);

  protected isFormOpen = signal(false);
  protected editingId = signal<number | null>(null);
  protected selectedCategoryIds = signal<Set<number>>(new Set());
  protected isSaving = signal(false);
  protected formError = signal<string | null>(null);
  protected form: GameForm = emptyForm();

  protected pendingDeleteId = signal<number | null>(null);
  protected busyGameId = signal<number | null>(null);

  protected allSelected = computed(() => {
    const games = this.games();
    return games.length > 0 && games.every((g) => this.selectedIds().has(g.id));
  });

  ngOnInit(): void {
    this.loadGames();
    this.loadCategories();
  }

  private async loadGames(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      this.games.set(await this.api.invoke(apiGameGet$Json));
    } catch {
      this.errorMessage.set('No se han podido cargar los juegos.');
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadCategories(): Promise<void> {
    try {
      this.categories.set(await this.api.invoke(apiCategoryGet$Json));
    } catch {
      this.categories.set([]);
    }
  }

  toggleSelect(id: number): void {
    this.selectedIds.update((ids) => {
      const next = new Set(ids);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  selectAll(): void {
    this.selectedIds.set(new Set(this.games().map((g) => g.id)));
  }

  deselectAll(): void {
    this.selectedIds.set(new Set());
  }

  async refreshSelected(): Promise<void> {
    const gameIds = Array.from(this.selectedIds());
    if (gameIds.length === 0) return;

    this.isRefreshing.set(true);
    this.refreshMessage.set(null);
    try {
      const updated = await this.api.invoke(apiGameRefreshPost$Json, { body: { gameIds } });
      const updatedById = new Map(updated.map((g) => [g.id, g]));
      this.games.update((games) => games.map((g) => updatedById.get(g.id) ?? g));
      this.refreshMessage.set(`Actualizados ${updated.length} de ${gameIds.length} juegos seleccionados.`);
    } catch {
      this.refreshMessage.set('No se han podido actualizar los juegos.');
    } finally {
      this.isRefreshing.set(false);
    }
  }

  openCreate(): void {
    this.form = emptyForm();
    this.editingId.set(null);
    this.selectedCategoryIds.set(new Set());
    this.formError.set(null);
    this.isFormOpen.set(true);
  }

  openEdit(game: GameDto): void {
    this.form = {
      name: game.name,
      releaseDate: game.releaseDate.slice(0, 10),
      developer: game.developer ?? '',
      publisher: game.publisher ?? '',
      description: game.description ?? '',
      coverImageUrl: game.coverImageUrl ?? '',
      metacritic: game.metacritic ?? null,
    };
    this.editingId.set(game.id);
    this.selectedCategoryIds.set(new Set(game.categories?.map((c) => c.id) ?? []));
    this.formError.set(null);
    this.isFormOpen.set(true);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  closeForm(): void {
    this.isFormOpen.set(false);
    this.editingId.set(null);
  }

  toggleCategory(categoryId: number): void {
    this.selectedCategoryIds.update((ids) => {
      const next = new Set(ids);
      if (next.has(categoryId)) {
        next.delete(categoryId);
      } else {
        next.add(categoryId);
      }
      return next;
    });
  }

  async saveGame(): Promise<void> {
    const name = this.form.name.trim();
    if (!name || !this.form.releaseDate) {
      this.formError.set('El nombre y la fecha de lanzamiento son obligatorios.');
      return;
    }

    const body = {
      name,
      releaseDate: this.form.releaseDate,
      developer: this.form.developer.trim() || null,
      publisher: this.form.publisher.trim() || null,
      description: this.form.description.trim() || null,
      coverImageUrl: this.form.coverImageUrl.trim() || null,
      metacritic: this.form.metacritic,
      categoryIds: Array.from(this.selectedCategoryIds()),
    };

    this.isSaving.set(true);
    this.formError.set(null);
    try {
      const id = this.editingId();
      if (id === null) {
        const created = await this.api.invoke(apiGamePost$Json, { body });
        this.games.update((games) => [created, ...games]);
      } else {
        const updated = await this.api.invoke(apiGameIdPut$Json, { id, body });
        this.games.update((games) => games.map((g) => (g.id === id ? updated : g)));
      }
      this.closeForm();
    } catch (err) {
      this.formError.set(extractErrorDetail(err) ?? 'No se ha podido guardar el juego.');
    } finally {
      this.isSaving.set(false);
    }
  }

  askDelete(id: number): void {
    this.pendingDeleteId.set(id);
  }

  cancelDelete(): void {
    this.pendingDeleteId.set(null);
  }

  async confirmDelete(game: GameDto): Promise<void> {
    this.busyGameId.set(game.id);
    this.errorMessage.set(null);
    try {
      await this.api.invoke(apiGameIdDelete, { id: game.id });
      this.games.update((games) => games.filter((g) => g.id !== game.id));
      this.selectedIds.update((ids) => {
        const next = new Set(ids);
        next.delete(game.id);
        return next;
      });
    } catch (err) {
      this.errorMessage.set(extractErrorDetail(err) ?? 'No se ha podido eliminar el juego.');
    } finally {
      this.busyGameId.set(null);
      this.pendingDeleteId.set(null);
    }
  }
}
