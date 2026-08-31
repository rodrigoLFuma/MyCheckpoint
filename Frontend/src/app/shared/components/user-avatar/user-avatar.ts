import { Component, input } from '@angular/core';

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
}
