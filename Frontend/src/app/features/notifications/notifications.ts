import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { Api } from '../../api/api';
import { apiNotificationGet$Json } from '../../api/fn/notification/api-notification-get-json';
import { apiNotificationMarkReadPost } from '../../api/fn/notification/api-notification-mark-read-post';
import { NotificationDto } from '../../api/models/notification-dto';
import { NotificationsService } from '../../core/services/notifications.service';
import { UserAvatar } from '../../shared/components/user-avatar/user-avatar';
import { Skeleton } from '../../shared/components/skeleton/skeleton';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-notifications',
  imports: [RouterLink, DatePipe, UserAvatar, Skeleton],
  templateUrl: './notifications.html',
})
export class Notifications implements OnInit {
  private api = inject(Api);
  private notificationsService = inject(NotificationsService);

  protected notifications = signal<NotificationDto[]>([]);
  protected isLoading = signal(true);
  protected isLoadingMore = signal(false);
  protected hasMore = signal(false);
  protected errorMessage = signal<string | null>(null);

  private loaded = 0;

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const notifications = await this.api.invoke(apiNotificationGet$Json, { skip: 0, take: PAGE_SIZE });
      this.notifications.set(notifications);
      this.loaded = notifications.length;
      this.hasMore.set(notifications.length === PAGE_SIZE);

      // Entrar aquí es lo que marca todo como leído: se limpia el badge de
      // la campana en la cabecera igual que se limpiaría en el servidor.
      await this.api.invoke(apiNotificationMarkReadPost);
      this.notificationsService.clear();
    } catch {
      this.errorMessage.set('No se han podido cargar las notificaciones.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async loadMore(): Promise<void> {
    this.isLoadingMore.set(true);
    try {
      const more = await this.api.invoke(apiNotificationGet$Json, { skip: this.loaded, take: PAGE_SIZE });
      this.notifications.update((current) => [...current, ...more]);
      this.loaded += more.length;
      this.hasMore.set(more.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se han podido cargar más notificaciones.');
    } finally {
      this.isLoadingMore.set(false);
    }
  }
}
