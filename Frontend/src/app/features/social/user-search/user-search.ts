import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiUserSearchGet$Json } from '../../../api/fn/user/api-user-search-get-json';
import { apiUserTopByFavoritesGet$Json } from '../../../api/fn/user/api-user-top-by-favorites-get-json';
import { apiCategoryGet$Json } from '../../../api/fn/category/api-category-get-json';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { CategoryDto } from '../../../api/models/category-dto';
import { CategoryTopUsersDto } from '../../../api/models/category-top-users-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';

@Component({
  selector: 'app-user-search',
  imports: [FormsModule, RouterLink, UserAvatar],
  templateUrl: './user-search.html',
})
export class UserSearch implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);

  term = '';
  selectedCategoryId: number | null = null;
  results = signal<UserSummaryDto[]>([]);
  isSearching = signal(false);
  errorMessage = signal<string | null>(null);
  hasSearched = signal(false);

  protected categories = signal<CategoryDto[]>([]);

  // Top por categorías favoritas: es lo que se ve por defecto, antes de
  // que el usuario busque nada explícitamente.
  protected topByFavorites = signal<CategoryTopUsersDto[]>([]);
  protected isLoadingTop = signal(false);

  ngOnInit(): void {
    this.loadCategories();
    if (this.authService.isLoggedIn()) {
      this.loadTopByFavorites();
    }
  }

  private async loadCategories(): Promise<void> {
    try {
      this.categories.set(await this.api.invoke(apiCategoryGet$Json));
    } catch {
      // Sin categorías, el filtro simplemente queda vacío.
    }
  }

  private async loadTopByFavorites(): Promise<void> {
    this.isLoadingTop.set(true);
    try {
      this.topByFavorites.set(await this.api.invoke(apiUserTopByFavoritesGet$Json));
    } catch {
      // Si falla, simplemente no se muestran recomendaciones.
    } finally {
      this.isLoadingTop.set(false);
    }
  }

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);
    this.isSearching.set(true);
    try {
      const found = await this.api.invoke(apiUserSearchGet$Json, {
        term: this.term.trim() || undefined,
        categoryId: this.selectedCategoryId ?? undefined,
      });
      this.results.set(found);
      this.hasSearched.set(true);
    } catch {
      this.errorMessage.set('No se ha podido buscar gente. Inténtalo de nuevo.');
    } finally {
      this.isSearching.set(false);
    }
  }
}
