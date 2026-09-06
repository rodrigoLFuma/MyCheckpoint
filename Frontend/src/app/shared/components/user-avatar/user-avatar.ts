import { Component, computed, input } from '@angular/core';
import { environment } from '../../../../environments/environment';

/**
 * Foto de perfil de un usuario, o el típico monigote gris si no tiene.
 * A diferencia de GameCard (que usa la inicial del nombre como respaldo),
 * para personas usamos una silueta genérica: no todos los nombres de
 * usuario empiezan por una letra que identifique a la persona.
 */
@Component({
  selector: 'app-user-avatar',
  templateUrl: './user-avatar.html',
})
export class UserAvatar {
  imageUrl = input<string | null | undefined>(null);
  name = input<string>('');
  sizeClass = input<string>('h-12 w-12');

  // Las fotos subidas por el usuario se guardan como ruta relativa al
  // backend (p. ej. "/uploads/profile-pictures/3_ab12.jpg"); una URL externa
  // pegada a mano (o de datos antiguos) ya viene absoluta y se deja tal cual.
  protected readonly resolvedUrl = computed(() => {
    const url = this.imageUrl();
    if (!url) return null;
    return url.startsWith('/') ? `${environment.apiUrl}${url}` : url;
  });
}
