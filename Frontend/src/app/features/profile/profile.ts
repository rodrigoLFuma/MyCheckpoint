import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { Api } from '../../api/api';
import { apiCategoryGet$Json } from '../../api/fn/category/api-category-get-json';
import { CategoryDto } from '../../api/models/category-dto';

/**
 * Editar tus datos públicos (nombre, avatar, bio, géneros favoritos). Se
 * llega aquí desde el botón "Editar perfil" de tu propio perfil público
 * (/users/:id); seguridad y borrar cuenta viven en /settings.
 */
@Component({
  selector: 'app-profile',
  imports: [FormsModule, RouterLink],
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);

  protected errorMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);
  protected saved = signal(false);

  protected categories = signal<CategoryDto[]>([]);
  protected selectedGenreIds = signal<Set<number>>(new Set());

  userName = '';
  profilePictureURL = '';
  bibliography = '';

  ngOnInit(): void {
    this.loadCategories();

    const user = this.authService.user();
    this.userName = user?.userName ?? '';
    this.profilePictureURL = user?.profilePictureURL ?? '';
    this.bibliography = user?.bibliography ?? '';
    this.selectedGenreIds.set(new Set(user?.favoriteGenres?.map((g) => g.id) ?? []));
  }

  private async loadCategories(): Promise<void> {
    try {
      this.categories.set(await this.api.invoke(apiCategoryGet$Json));
    } catch {
      // Sin categorías, el selector de géneros favoritos queda vacío.
    }
  }

  toggleGenre(categoryId: number): void {
    this.selectedGenreIds.update((ids) => {
      const next = new Set(ids);
      if (next.has(categoryId)) {
        next.delete(categoryId);
      } else {
        next.add(categoryId);
      }
      return next;
    });
  }

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);
    this.saved.set(false);
    this.isSubmitting.set(true);
    try {
      await this.authService.updateProfile({
        userName: this.userName,
        profilePictureURL: this.profilePictureURL || null,
        bibliography: this.bibliography || null,
        favoriteGenreCategoryIds: Array.from(this.selectedGenreIds()),
      });
      this.saved.set(true);
    } catch {
      this.errorMessage.set('No se ha podido guardar el perfil.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
