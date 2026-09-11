import { Component, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-game-card',
  imports: [DatePipe, RouterLink],
  templateUrl: './game-card.html',
})
export class GameCard {
  name = input.required<string>();
  imageUrl = input<string | null | undefined>(null);
  releaseDate = input<string | Date | null | undefined>(null);
  link = input<string | unknown[] | null | undefined>(null);
}
