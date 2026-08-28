import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  userName = '';
  email = '';
  password = '';
  errorMessage = signal<string | null>(null);
  isSubmitting = signal(false);

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);
    this.isSubmitting.set(true);
    try {
      await this.authService.register({
        userName: this.userName,
        email: this.email,
        password: this.password,
      });
      this.router.navigateByUrl('/');
    } catch (err) {
      const detail = err instanceof HttpErrorResponse ? err.error?.detail : null;
      this.errorMessage.set(detail ?? 'No se ha podido completar el registro.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
