import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Api } from '../../../api/api';
import { apiUserCategoryCategoryIdTopGet$Json } from '../../../api/fn/user/api-user-category-category-id-top-get-json';
import { apiCategoryIdGet$Json } from '../../../api/fn/category/api-category-id-get-json';
import { UserSummaryDto } from '../../../api/models/user-summary-dto';
import { CategoryDto } from '../../../api/models/category-dto';
import { UserAvatar } from '../../../shared/components/user-avatar/user-avatar';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-category-top',
  imports: [RouterLink, UserAvatar],
  templateUrl: './category-top.html',
})
export class CategoryTop implements OnInit {
  private api = inject(Api);
  private route = inject(ActivatedRoute);

  protected category = signal<CategoryDto | null>(null);
  protected users = signal<UserSummaryDto[]>([]);
  protected isLoading = signal(true);
  protected isLoadingMore = signal(false);
  protected hasMore = signal(false);
  protected errorMessage = signal<string | null>(null);

  private categoryId = 0;
  private loaded = 0;

  ngOnInit(): void {
    this.categoryId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const [category, users] = await Promise.all([
        this.api.invoke(apiCategoryIdGet$Json, { id: this.categoryId }),
        this.api.invoke(apiUserCategoryCategoryIdTopGet$Json, { categoryId: this.categoryId, skip: 0, take: PAGE_SIZE }),
      ]);
      this.category.set(category);
      this.users.set(users);
      this.loaded = users.length;
      this.hasMore.set(users.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se ha podido cargar el ranking.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async loadMore(): Promise<void> {
    this.isLoadingMore.set(true);
    try {
      const more = await this.api.invoke(apiUserCategoryCategoryIdTopGet$Json, {
        categoryId: this.categoryId,
        skip: this.loaded,
        take: PAGE_SIZE,
      });
      this.users.update((current) => [...current, ...more]);
      this.loaded += more.length;
      this.hasMore.set(more.length === PAGE_SIZE);
    } catch {
      this.errorMessage.set('No se han podido cargar más resultados.');
    } finally {
      this.isLoadingMore.set(false);
    }
  }
}
