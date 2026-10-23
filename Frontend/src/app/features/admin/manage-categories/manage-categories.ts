import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../../api/api';
import { apiCategoryGet$Json } from '../../../api/fn/category/api-category-get-json';
import { apiCategoryPost$Json } from '../../../api/fn/category/api-category-post-json';
import { apiCategoryIdPut$Json } from '../../../api/fn/category/api-category-id-put-json';
import { apiCategoryIdDelete } from '../../../api/fn/category/api-category-id-delete';
import { apiCategoryImportFromRawgPost$Json } from '../../../api/fn/category/api-category-import-from-rawg-post-json';
import { CategoryDto } from '../../../api/models/category-dto';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';
import { extractErrorDetail } from '../../../shared/utils/http-error';

@Component({
  selector: 'app-manage-categories',
  imports: [FormsModule, Skeleton],
  templateUrl: './manage-categories.html',
})
export class ManageCategories implements OnInit {
  private api = inject(Api);

  protected categories = signal<CategoryDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);
  protected infoMessage = signal<string | null>(null);

  protected newName = '';
  protected isCreating = signal(false);

  protected editingId = signal<number | null>(null);
  protected editName = '';

  protected pendingDeleteId = signal<number | null>(null);
  protected busyId = signal<number | null>(null);
  protected isSyncing = signal(false);

  ngOnInit(): void {
    this.loadCategories();
  }

  private async loadCategories(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      this.categories.set(await this.api.invoke(apiCategoryGet$Json));
    } catch {
      this.errorMessage.set('No se han podido cargar las categorías.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async createCategory(): Promise<void> {
    const name = this.newName.trim();
    if (!name) return;

    this.isCreating.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
    try {
      const created = await this.api.invoke(apiCategoryPost$Json, { body: { name } });
      this.categories.update((list) => [...list, created].sort((a, b) => a.name.localeCompare(b.name)));
      this.newName = '';
    } catch (err) {
      this.errorMessage.set(extractErrorDetail(err) ?? 'No se ha podido crear la categoría.');
    } finally {
      this.isCreating.set(false);
    }
  }

  startEdit(category: CategoryDto): void {
    this.editingId.set(category.id);
    this.editName = category.name;
    this.pendingDeleteId.set(null);
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  async saveEdit(category: CategoryDto): Promise<void> {
    const name = this.editName.trim();
    if (!name) return;

    this.busyId.set(category.id);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
    try {
      const updated = await this.api.invoke(apiCategoryIdPut$Json, { id: category.id, body: { name } });
      this.categories.update((list) =>
        list.map((c) => (c.id === category.id ? updated : c)).sort((a, b) => a.name.localeCompare(b.name)),
      );
      this.editingId.set(null);
    } catch (err) {
      this.errorMessage.set(extractErrorDetail(err) ?? 'No se ha podido renombrar la categoría.');
    } finally {
      this.busyId.set(null);
    }
  }

  askDelete(id: number): void {
    this.pendingDeleteId.set(id);
    this.editingId.set(null);
  }

  cancelDelete(): void {
    this.pendingDeleteId.set(null);
  }

  async confirmDelete(category: CategoryDto): Promise<void> {
    this.busyId.set(category.id);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
    try {
      await this.api.invoke(apiCategoryIdDelete, { id: category.id });
      this.categories.update((list) => list.filter((c) => c.id !== category.id));
    } catch (err) {
      this.errorMessage.set(extractErrorDetail(err) ?? 'No se ha podido eliminar la categoría.');
    } finally {
      this.busyId.set(null);
      this.pendingDeleteId.set(null);
    }
  }

  async syncFromRawg(): Promise<void> {
    this.isSyncing.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
    try {
      const result = await this.api.invoke(apiCategoryImportFromRawgPost$Json);
      this.categories.set([...result].sort((a, b) => a.name.localeCompare(b.name)));
      this.infoMessage.set('Categorías sincronizadas con RAWG.');
    } catch {
      this.errorMessage.set('No se han podido sincronizar las categorías con RAWG.');
    } finally {
      this.isSyncing.set(false);
    }
  }
}
