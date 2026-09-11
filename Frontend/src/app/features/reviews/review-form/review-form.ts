import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Api } from '../../../api/api';
import { apiGameIdGet$Json } from '../../../api/fn/game/api-game-id-get-json';
import { apiUserGameIdGet$Json } from '../../../api/fn/user-game/api-user-game-id-get-json';
import { apiUserGamePost$Json } from '../../../api/fn/user-game/api-user-game-post-json';
import { apiUserGameIdPut$Json } from '../../../api/fn/user-game/api-user-game-id-put-json';
import { apiUserGameIdDelete } from '../../../api/fn/user-game/api-user-game-id-delete';
import { apiGameListPendingItemsPost } from '../../../api/fn/game-list/api-game-list-pending-items-post';
import { GameDto } from '../../../api/models/game-dto';
import { AuthService } from '../../../core/services/auth.service';
import { extractErrorDetail } from '../../../shared/utils/http-error';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';
import { Select, SelectOption } from '../../../shared/components/select/select';
import { RatingPicker } from '../../../shared/components/rating-picker/rating-picker';

const CREATE_STATUSES = ['Jugando', 'Completado', 'Abandonado', 'Pendiente'] as const;
const EDIT_STATUSES = ['Jugando', 'Completado', 'Abandonado'] as const;
const REVIEW_MAX_LENGTH = 2000;

@Component({
  selector: 'app-review-form',
  imports: [FormsModule, DatePipe, Skeleton, Select, RatingPicker],
  templateUrl: './review-form.html',
})
export class ReviewForm implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(Api);
  private location = inject(Location);
  protected authService = inject(AuthService);

  protected readonly reviewMaxLength = REVIEW_MAX_LENGTH;

  protected isEditMode = signal(false);
  protected game = signal<GameDto | null>(null);
  protected isLoading = signal(true);
  protected loadError = signal<string | null>(null);
  protected isForbidden = signal(false);
  protected isSubmitting = signal(false);
  protected submitError = signal<string | null>(null);

  protected pendingDelete = signal(false);
  protected isDeleting = signal(false);
  protected deleteError = signal<string | null>(null);

  private gameId: number | null = null;
  private userGameId: number | null = null;

  status = 'Jugando';
  rate = 5;
  title = '';
  review = '';

  protected get statuses(): readonly string[] {
    return this.isEditMode() ? EDIT_STATUSES : CREATE_STATUSES;
  }

  protected get statusOptions(): SelectOption[] {
    return this.statuses.map((status) => ({ value: status, label: status }));
  }

  ngOnInit(): void {
    const editId = this.route.snapshot.paramMap.get('id');
    const newGameId = this.route.snapshot.paramMap.get('gameId');

    if (editId) {
      this.isEditMode.set(true);
      this.userGameId = Number(editId);
      this.loadForEdit(this.userGameId);
    } else if (newGameId) {
      this.gameId = Number(newGameId);
      this.loadForCreate(this.gameId);
    } else {
      this.loadError.set('No se ha podido determinar el juego.');
      this.isLoading.set(false);
    }
  }

  private async loadForCreate(gameId: number): Promise<void> {
    this.isLoading.set(true);
    try {
      this.game.set(await this.api.invoke(apiGameIdGet$Json, { id: gameId }));
    } catch {
      this.loadError.set('No se ha podido cargar el juego.');
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadForEdit(id: number): Promise<void> {
    this.isLoading.set(true);
    try {
      const userGame = await this.api.invoke(apiUserGameIdGet$Json, { id });

      if (userGame.userSummary.id !== this.authService.user()?.id) {
        this.isForbidden.set(true);
        return;
      }

      this.game.set(userGame.game);
      this.status = (EDIT_STATUSES as readonly string[]).includes(userGame.status) ? userGame.status : 'Jugando';
      this.rate = userGame.rate ?? 5;
      this.title = userGame.title ?? '';
      this.review = userGame.review ?? '';
    } catch {
      this.loadError.set('No se ha podido cargar la reseña.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async onSubmit(): Promise<void> {
    this.submitError.set(null);
    this.isSubmitting.set(true);
    try {
      if (this.isEditMode()) {
        await this.api.invoke(apiUserGameIdPut$Json, {
          id: this.userGameId!,
          body: { status: this.status, rate: this.rate, title: this.title || null, review: this.review || null },
        });
      } else if (this.status === 'Pendiente') {

        await this.api.invoke(apiGameListPendingItemsPost, { body: { gameId: this.gameId! } });
      } else {
        await this.api.invoke(apiUserGamePost$Json, {
          body: {
            gameId: this.gameId!,
            status: this.status,
            rate: this.rate,
            title: this.title || null,
            review: this.review || null,
          },
        });
      }

      this.router.navigateByUrl('/library');
    } catch (err) {
      this.submitError.set(extractErrorDetail(err) ?? 'No se ha podido guardar la reseña.');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  goBack(): void {
    this.location.back();
  }

  askDelete(): void {
    this.deleteError.set(null);
    this.pendingDelete.set(true);
  }

  cancelDelete(): void {
    this.pendingDelete.set(false);
  }

  async confirmDelete(): Promise<void> {
    this.isDeleting.set(true);
    this.deleteError.set(null);
    try {
      await this.api.invoke(apiUserGameIdDelete, { id: this.userGameId! });
      this.router.navigateByUrl('/library');
    } catch {
      this.deleteError.set('No se ha podido borrar la reseña.');
      this.isDeleting.set(false);
    }
  }
}
