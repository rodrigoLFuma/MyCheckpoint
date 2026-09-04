import { Injectable, inject, signal } from '@angular/core';
import { Api } from '../../api/api';
import { apiNotificationUnreadCountGet$Json } from '../../api/fn/notification/api-notification-unread-count-get-json';

/**
 * Contador de notificaciones sin leer, cargado una vez al iniciar sesión
 * (no hay polling ni tiempo real: se refresca al entrar y al visitar
 * /notifications, que las marca todas como leídas).
 */
@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private api = inject(Api);

  private readonly _unreadCount = signal(0);
  readonly unreadCount = this._unreadCount.asReadonly();

  async refreshUnreadCount(): Promise<void> {
    try {
      this._unreadCount.set(await this.api.invoke(apiNotificationUnreadCountGet$Json));
    } catch {
      // Sin sesión válida u otro fallo: no hay contador que mostrar.
    }
  }

  clear(): void {
    this._unreadCount.set(0);
  }
}
