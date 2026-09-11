import { Component, computed, input } from '@angular/core';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-user-avatar',
  templateUrl: './user-avatar.html',
})
export class UserAvatar {
  imageUrl = input<string | null | undefined>(null);
  name = input<string>('');
  sizeClass = input<string>('h-12 w-12');

  protected readonly resolvedUrl = computed(() => {
    const url = this.imageUrl();
    if (!url) return null;
    return url.startsWith('/') ? `${environment.apiUrl}${url}` : url;
  });
}
