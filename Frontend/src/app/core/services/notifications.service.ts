import { Injectable, inject, signal } from '@angular/core';
import { Api } from '../../api/api';
import { apiNotificationUnreadCountGet$Json } from '../../api/fn/notification/api-notification-unread-count-get-json';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private api = inject(Api);

  private readonly _unreadCount = signal(0);
  readonly unreadCount = this._unreadCount.asReadonly();

  async refreshUnreadCount(): Promise<void> {
    try {
      this._unreadCount.set(await this.api.invoke(apiNotificationUnreadCountGet$Json));
    } catch {

    }
  }

  clear(): void {
    this._unreadCount.set(0);
  }
}
