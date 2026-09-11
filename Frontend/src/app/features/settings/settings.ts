import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { extractErrorDetail } from '../../shared/utils/http-error';

@Component({
  selector: 'app-settings',
  imports: [FormsModule],
  templateUrl: './settings.html',
})
export class Settings {
  private authService = inject(AuthService);
  private router = inject(Router);

  protected isChangingPassword = signal(false);
  protected isSubmittingPassword = signal(false);
  protected passwordError = signal<string | null>(null);
  protected passwordSuccess = signal(false);

  protected pendingDeleteAccount = signal(false);
  protected isDeletingAccount = signal(false);
  protected deleteError = signal<string | null>(null);

  currentPassword = '';
  newPassword = '';
  confirmNewPassword = '';

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
