import { Component, effect, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { NotificationsService } from './core/services/notifications.service';
import { UserAvatar } from './shared/components/user-avatar/user-avatar';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, UserAvatar],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected authService = inject(AuthService);
  protected notificationsService = inject(NotificationsService);
  private router = inject(Router);

  protected readonly isMenuOpen = signal(false);

  constructor() {
    effect(() => {
      if (this.authService.isLoggedIn()) {
        this.notificationsService.refreshUnreadCount();
      } else {
        this.notificationsService.clear();
      }
    });
  }

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

      this.authService.clearLocalSession();
      this.router.navigateByUrl('/login');
    }
  }
}
