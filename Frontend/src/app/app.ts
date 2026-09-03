import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected authService = inject(AuthService);
  private router = inject(Router);

  protected readonly isMenuOpen = signal(false);

  toggleMenu(): void {
    this.isMenuOpen.update((open) => !open);
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
  }

  async logout(): Promise<void> {
    this.closeMenu();
    try {
      await this.authService.logout();
    } finally {
      // Aunque la llamada al backend falle, cerramos la sesión en el
      // cliente igualmente: la cookie HttpOnly expira sola y lo que
      // importa es que la UI refleje la intención del usuario.
      this.authService.clearLocalSession();
      this.router.navigateByUrl('/login');
    }
  }
}
