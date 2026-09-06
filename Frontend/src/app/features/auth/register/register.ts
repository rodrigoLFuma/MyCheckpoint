import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { extractErrorDetail } from '../../../shared/utils/http-error';

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
      this.errorMessage.set(extractErrorDetail(err) ?? 'No se ha podido completar el registro.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
