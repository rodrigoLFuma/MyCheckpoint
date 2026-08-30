import { Injectable, inject, signal, computed } from '@angular/core';
import { Api } from '../../api/api';
import { apiAuthLoginPost$Json } from '../../api/fn/auth/api-auth-login-post-json';
import { apiAuthRegisterPost$Json } from '../../api/fn/auth/api-auth-register-post-json';
import { apiAuthLogoutPost } from '../../api/fn/auth/api-auth-logout-post';
import { apiUserMeGet$Json } from '../../api/fn/user/api-user-me-get-json';
import { apiUserMePut$Json } from '../../api/fn/user/api-user-me-put-json';
import { apiUserMeDelete } from '../../api/fn/user/api-user-me-delete';
import { LoginDto } from '../../api/models/login-dto';
import { UserAddDto } from '../../api/models/user-add-dto';
import { UserDto } from '../../api/models/user-dto';
import { UserModifyDto } from '../../api/models/user-modify-dto';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = inject(Api);

  private readonly _user = signal<UserDto | null>(null);
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);
  readonly isAdmin = computed(() => this._user()?.roles?.includes('Admin') ?? false);

  async login(dto: LoginDto): Promise<void> {
    const response = await this.api.invoke(apiAuthLoginPost$Json, { body: dto });
    this._user.set(response.user ?? null);
  }

  async register(dto: UserAddDto): Promise<void> {
    const response = await this.api.invoke(apiAuthRegisterPost$Json, { body: dto });
    this._user.set(response.user ?? null);
  }

  async updateProfile(dto: UserModifyDto): Promise<void> {
    const response = await this.api.invoke(apiUserMePut$Json, { body: dto });
    this._user.set(response);
  }

  async logout(): Promise<void> {
    await this.api.invoke(apiAuthLogoutPost);
    this._user.set(null);
  }

  async deleteAccount(): Promise<void> {
    await this.api.invoke(apiUserMeDelete);
    try {
      // La cuenta ya no existe, pero la cookie HttpOnly sigue en el
      // navegador hasta que el propio backend la borre explícitamente.
      await this.api.invoke(apiAuthLogoutPost);
    } finally {
      this._user.set(null);
    }
  }

  /**
   * Limpia la sesión solo en el cliente, sin llamar al backend. Úsalo cuando
   * ya sabes que la sesión dejó de ser válida ahí (p. ej. un 401 inesperado),
   * no como sustituto de logout().
   */
  clearLocalSession(): void {
    this._user.set(null);
  }

  async checkSession(): Promise<void> {
    try {
      const user = await this.api.invoke(apiUserMeGet$Json);
      this._user.set(user);
    } catch {
      this._user.set(null);
    }
  }
}
