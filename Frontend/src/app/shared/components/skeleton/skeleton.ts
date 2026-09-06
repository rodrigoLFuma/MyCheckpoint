import { Component } from '@angular/core';

/**
 * Bloque gris con animación de pulso, sin forma propia: la forma la da quien
 * lo usa con clases de Tailwind normales, p. ej. `<app-skeleton class="h-4
 * w-32" />` para una línea de texto, `class="h-10 w-10 rounded-full"` para un
 * avatar, o `class="h-40 w-full"` para la imagen de una tarjeta. Angular
 * mezcla esas clases con las del host, así que no hace falta ningún input.
 */
@Component({
  selector: 'app-skeleton',
  template: '',
  host: { class: 'block animate-pulse rounded bg-slate-200' },
})
export class Skeleton {}
