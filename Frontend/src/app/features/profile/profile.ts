import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Api } from '../../api/api';
import { apiCategoryGet$Json } from '../../api/fn/category/api-category-get-json';
import { CategoryDto } from '../../api/models/category-dto';

/**
 * Los endpoints que devuelven 204 No Content (como cambiar contraseña) piden
 * `responseType: 'text'`, así que Angular no parsea el cuerpo del error como
 * JSON automáticamente (solo lo hace cuando responseType es 'json'): aquí
 * `err.error` llega como texto plano, no como objeto. Hay que intentar
 * parsearlo a mano para poder leer el `detail` del ProblemDetails.
 */
function extractErrorDetail(err: unknown): string | null {
  if (!(err instanceof HttpErrorResponse)) return null;
  const raw = err.error;
  if (raw && typeof raw === 'object') return raw.detail ?? null;
  if (typeof raw === 'string') {
    try {
      return JSON.parse(raw)?.detail ?? null;
    } catch {
      return null;
    }
  }
  return null;
}

@Component({
  selector: 'app-profile',
  imports: [FormsModule, DatePipe],
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);
  private router = inject(Router);

  protected isEditing = signal(false);
  protected errorMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);

  protected isChangingPassword = signal(false);
  protected isSubmittingPassword = signal(false);
  protected passwordError = signal<string | null>(null);
  protected passwordSuccess = signal(false);

  protected pendingDeleteAccount = signal(false);
  protected isDeletingAccount = signal(false);
  protected deleteError = signal<string | null>(null);

  protected categories = signal<CategoryDto[]>([]);
  protected selectedGenreIds = signal<Set<number>>(new Set());

  userName = '';
  profilePictureURL = '';
  bibliography = '';

  currentPassword = '';
  newPassword = '';
  confirmNewPassword = '';

  ngOnInit(): void {
    this.loadCategories();
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

  startEditing(): void {
    const user = this.authService.user();
    this.userName = user?.userName ?? '';
    this.profilePictureURL = user?.profilePictureURL ?? '';
    this.bibliography = user?.bibliography ?? '';
    this.selectedGenreIds.set(new Set(user?.favoriteGenres?.map((g) => g.id) ?? []));
    this.errorMessage.set(null);
    this.isEditing.set(true);
  }

  cancelEditing(): void {
    this.isEditing.set(false);
  }

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);
    this.isSubmitting.set(true);
    try {
      await this.authService.updateProfile({
        userName: this.userName,
        profilePictureURL: this.profilePictureURL || null,
        bibliography: this.bibliography || null,
        favoriteGenreCategoryIds: Array.from(this.selectedGenreIds()),
      });
      this.isEditing.set(false);
    } catch {
      this.errorMessage.set('No se ha podido guardar el perfil.');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  startChangePassword(): void {
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmNewPassword = '';
    this.passwordError.set(null);
    this.passwordSuccess.set(false);
    this.isChangingPassword.set(true);
  }

  cancelChangePassword(): void {
    this.isChangingPassword.set(false);
  }

  async onChangePasswordSubmit(): Promise<void> {
    this.passwordError.set(null);

    if (this.newPassword !== this.confirmNewPassword) {
      this.passwordError.set('Las contraseñas nuevas no coinciden.');
      return;
    }

    this.isSubmittingPassword.set(true);
    try {
      await this.authService.changePassword({
        currentPassword: this.currentPassword,
        newPassword: this.newPassword,
      });
      this.passwordSuccess.set(true);
      this.isChangingPassword.set(false);
    } catch (err) {
      this.passwordError.set(extractErrorDetail(err) ?? 'No se ha podido cambiar la contraseña.');
    } finally {
      this.isSubmittingPassword.set(false);
    }
  }

  askDeleteAccount(): void {
    this.deleteError.set(null);
    this.pendingDeleteAccount.set(true);
  }

  cancelDeleteAccount(): void {
    this.pendingDeleteAccount.set(false);
  }

  async confirmDeleteAccount(): Promise<void> {
    this.isDeletingAccount.set(true);
    this.deleteError.set(null);
    try {
      await this.authService.deleteAccount();
      this.router.navigateByUrl('/');
    } catch {
      this.deleteError.set('No se ha podido borrar la cuenta.');
      this.isDeletingAccount.set(false);
    }
  }
}
