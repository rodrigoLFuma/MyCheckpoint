import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { Api } from '../../api/api';
import { apiCategoryGet$Json } from '../../api/fn/category/api-category-get-json';
import { CategoryDto } from '../../api/models/category-dto';
import { UserAvatar } from '../../shared/components/user-avatar/user-avatar';
import { extractErrorDetail } from '../../shared/utils/http-error';

/**
 * Editar tus datos públicos (nombre, avatar, bio, géneros favoritos). Se
 * llega aquí desde el botón "Editar perfil" de tu propio perfil público
 * (/users/:id); seguridad y borrar cuenta viven en /settings.
 */
@Component({
  selector: 'app-profile',
  imports: [FormsModule, RouterLink, UserAvatar],
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);

  protected errorMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);
  protected saved = signal(false);

  // La foto de perfil tiene su propio endpoint (sube/borra un archivo) y por
  // tanto su propio estado de carga/error, independiente del resto del
  // formulario (nombre, bio, géneros), que se guarda aparte con "Guardar".
  protected isUploadingPicture = signal(false);
  protected pictureError = signal<string | null>(null);

  protected categories = signal<CategoryDto[]>([]);
  protected selectedGenreIds = signal<Set<number>>(new Set());

  userName = '';
  bibliography = '';

  ngOnInit(): void {
    this.loadCategories();

    const user = this.authService.user();
    this.userName = user?.userName ?? '';
    this.bibliography = user?.bibliography ?? '';
    this.selectedGenreIds.set(new Set(user?.favoriteGenres?.map((g) => g.id) ?? []));
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.pictureError.set(null);
    this.isUploadingPicture.set(true);
    try {
      await this.authService.uploadProfilePicture(file);
    } catch (err) {
      this.pictureError.set(extractErrorDetail(err) ?? 'No se ha podido subir la imagen.');
    } finally {
      this.isUploadingPicture.set(false);
      // Para poder volver a elegir el mismo archivo si se quiere reintentar.
      input.value = '';
    }
  }

  async removePicture(): Promise<void> {
    this.pictureError.set(null);
    this.isUploadingPicture.set(true);
    try {
      await this.authService.removeProfilePicture();
    } catch {
      this.pictureError.set('No se ha podido quitar la foto.');
    } finally {
      this.isUploadingPicture.set(false);
    }
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
