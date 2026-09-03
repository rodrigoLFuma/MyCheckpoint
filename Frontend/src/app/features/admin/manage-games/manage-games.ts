import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Api } from '../../../api/api';
import { apiGameGet$Json } from '../../../api/fn/game/api-game-get-json';
import { apiGameRefreshPost$Json } from '../../../api/fn/game/api-game-refresh-post-json';
import { GameDto } from '../../../api/models/game-dto';

/**
 * Herramienta admin para refrescar desde RAWG los datos de juegos ya
 * importados (se quedan congelados desde el momento en que se importaron:
 * nombre, portada, descripción, Metacritic, géneros...). Cada juego
 * seleccionado supone una llamada a RAWG, así que la selección es
 * deliberada, no automática.
 */
@Component({
  selector: 'app-manage-games',
  imports: [DatePipe],
  templateUrl: './manage-games.html',
})
export class ManageGames implements OnInit {
  private api = inject(Api);

  protected games = signal<GameDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected selectedIds = signal<Set<number>>(new Set());
  protected isRefreshing = signal(false);
  protected refreshMessage = signal<string | null>(null);

  protected allSelected = computed(() => {
    const games = this.games();
    return games.length > 0 && games.every((g) => this.selectedIds().has(g.id));
  });

  ngOnInit(): void {
    this.loadGames();
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
}
