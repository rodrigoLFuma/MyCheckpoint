import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { AuthService } from '../../../core/services/auth.service';
import { apiUserGet$Json } from '../../../api/fn/user/api-user-get-json';
import { apiUserIdDelete } from '../../../api/fn/user/api-user-id-delete';
import { apiUserIdAdminRolePut$Json } from '../../../api/fn/user/api-user-id-admin-role-put-json';
import { UserDto } from '../../../api/models/user-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';

const ADMIN_ROLE = 'Admin';

@Component({
  selector: 'app-manage-users',
  imports: [DatePipe, RouterLink, UserAvatar, Skeleton],
  templateUrl: './manage-users.html',
})
export class ManageUsers implements OnInit {
  protected authService = inject(AuthService);
  private api = inject(Api);

  protected users = signal<UserDto[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected busyUserId = signal<number | null>(null);
  protected pendingDeleteId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadUsers();
  }

  private async loadUsers(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      this.users.set(await this.api.invoke(apiUserGet$Json));
    } catch {
      this.errorMessage.set('No se han podido cargar los usuarios.');
    } finally {
      this.isLoading.set(false);
    }
  }

  isAdmin(user: UserDto): boolean {
    return user.roles?.includes(ADMIN_ROLE) ?? false;
  }

  isSelf(user: UserDto): boolean {
    return user.id === this.authService.user()?.id;
  }

  async toggleAdmin(user: UserDto): Promise<void> {
    this.busyUserId.set(user.id);
    this.errorMessage.set(null);
    try {
      const updated = await this.api.invoke(apiUserIdAdminRolePut$Json, {
        id: user.id,
        body: { isAdmin: !this.isAdmin(user) },
      });
      this.users.update((users) => users.map((u) => (u.id === user.id ? updated : u)));
    } catch {
      this.errorMessage.set('No se ha podido cambiar el rol.');
    } finally {
      this.busyUserId.set(null);
    }
  }

  askDelete(userId: number): void {
    this.pendingDeleteId.set(userId);
  }

  cancelDelete(): void {
    this.pendingDeleteId.set(null);
  }

  async confirmDelete(user: UserDto): Promise<void> {
    this.busyUserId.set(user.id);
    this.errorMessage.set(null);
    try {
      await this.api.invoke(apiUserIdDelete, { id: user.id });
      this.users.update((users) => users.filter((u) => u.id !== user.id));
    } catch {
      this.errorMessage.set('No se ha podido eliminar el usuario.');
    } finally {
      this.busyUserId.set(null);
      this.pendingDeleteId.set(null);
    }
  }
}
