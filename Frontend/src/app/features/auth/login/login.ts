import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  emailOrUserName = '';
  password = '';
  errorMessage = signal<string | null>(null);
  isSubmitting = signal(false);

  async onSubmit(): Promise<void> {
    this.errorMessage.set(null);
    this.isSubmitting.set(true);
    try {
      await this.authService.login({ emailOrUserName: this.emailOrUserName, password: this.password });

      const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
      this.router.navigateByUrl(returnUrl || '/');
    } catch {
      this.errorMessage.set('Credenciales inválidas.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
