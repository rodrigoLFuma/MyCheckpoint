import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  errorMessage = signal<string | null>(null);
  isSubmitting = signal(false);

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);
    this.isSubmitting.set(true);
    try {
      await this.authService.login({ email: this.email, password: this.password });
      this.router.navigateByUrl('/');
    } catch {
      this.errorMessage.set('Credenciales inválidas.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
