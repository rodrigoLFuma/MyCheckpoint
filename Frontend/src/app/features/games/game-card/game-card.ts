import { Component, input } from '@angular/core';
import { DatePipe } from '@angular/common';

/**
 * Tarjeta de juego puramente visual: no sabe si el juego viene de RAWG o de
 * la base de datos local, solo pinta lo que le pasan. Así sirve tanto para
 * resultados de búsqueda externa como para juegos ya importados, sin
 * depender de un tipo compartido entre backend y frontend.
 */
@Component({
  selector: 'app-game-card',
  imports: [DatePipe],
  templateUrl: './game-card.html',
})
export class GameCard {
  name = input.required<string>();
  imageUrl = input<string | null | undefined>(null);
  releaseDate = input<string | Date | null | undefined>(null);
}
