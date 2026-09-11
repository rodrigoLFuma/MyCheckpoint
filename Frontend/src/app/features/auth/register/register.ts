import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Api } from '../../../api/api';
import { apiCategoryGet$Json } from '../../../api/fn/category/api-category-get-json';
import { CategoryDto } from '../../../api/models/category-dto';
import { extractErrorDetail } from '../../../shared/utils/http-error';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register implements OnInit {
  private authService = inject(AuthService);
  private api = inject(Api);
  private router = inject(Router);

  userName = '';
  email = '';
  password = '';
  errorMessage = signal<string | null>(null);
  isSubmitting = signal(false);

  protected step = signal<1 | 2>(1);

  protected categories = signal<CategoryDto[]>([]);
  protected selectedGenreIds = signal<Set<number>>(new Set());

  ngOnInit(): void {
    this.loadCategories();
  }

  goToStep2(): void {
    this.errorMessage.set(null);

    if (!this.userName.trim() || !this.email.trim()) {
      this.errorMessage.set('Rellena tu nombre de usuario y tu email.');
      return;
    }
    if (!/^(?=.*[A-Z])(?=.*\d).{8,}$/.test(this.password)) {
      this.errorMessage.set('La contraseña debe tener mínimo 8 caracteres, con al menos una mayúscula y un número.');
      return;
    }

    this.step.set(2);
  }

  backToStep1(): void {
    this.errorMessage.set(null);
    this.step.set(1);
  }

  private async loadCategories(): Promise<void> {
    try {
      this.categories.set(await this.api.invoke(apiCategoryGet$Json));
    } catch {

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
    this.isSubmitting.set(true);
    try {
      await this.authService.register({
        userName: this.userName,
        email: this.email,
        password: this.password,
      });

      if (this.selectedGenreIds().size > 0) {
        try {
          await this.authService.updateProfile({
            userName: this.userName,
            favoriteGenreCategoryIds: Array.from(this.selectedGenreIds()),
          });
        } catch {

        }
      }

      this.router.navigateByUrl('/');
    } catch (err) {
      this.errorMessage.set(extractErrorDetail(err) ?? 'No se ha podido completar el registro.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
