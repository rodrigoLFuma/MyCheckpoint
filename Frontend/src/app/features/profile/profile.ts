import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  imports: [FormsModule, DatePipe],
  templateUrl: './profile.html',
})
export class Profile {
  protected authService = inject(AuthService);

  protected isEditing = signal(false);
  protected errorMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);

  userName = '';
  profilePictureURL = '';
  bibliography = '';

  startEditing(): void {
    const user = this.authService.user();
    this.userName = user?.userName ?? '';
    this.profilePictureURL = user?.profilePictureURL ?? '';
    this.bibliography = user?.bibliography ?? '';
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
      });
      this.isEditing.set(false);
    } catch {
      this.errorMessage.set('No se ha podido guardar el perfil.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
