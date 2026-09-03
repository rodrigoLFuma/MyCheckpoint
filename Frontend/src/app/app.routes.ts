import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./features/home/home').then((m) => m.Home),
  },
  { path: 'login', loadComponent: () => import('./features/auth/login/login').then((m) => m.Login) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register').then((m) => m.Register) },
  {
    path: 'games/search',
    loadComponent: () => import('./features/games/search/game-search').then((m) => m.GameSearch),
  },
  {
    path: 'social',
    loadComponent: () => import('./features/social/user-search/user-search').then((m) => m.UserSearch),
  },
  {
    path: 'social/activity',
    canActivate: [authGuard],
    loadComponent: () => import('./features/social/activity-feed/activity-feed').then((m) => m.ActivityFeed),
  },
  {
    path: 'social/category/:id',
    loadComponent: () => import('./features/social/category-top/category-top').then((m) => m.CategoryTop),
  },
  {
    path: 'reviews/new/:gameId',
    canActivate: [authGuard],
    loadComponent: () => import('./features/reviews/review-form/review-form').then((m) => m.ReviewForm),
  },
  {
    path: 'reviews/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./features/reviews/review-form/review-form').then((m) => m.ReviewForm),
  },
  {
    path: 'reviews/:id',
    loadComponent: () => import('./features/reviews/review-detail/review-detail').then((m) => m.ReviewDetail),
  },
  {
    path: 'users/:id',
    loadComponent: () => import('./features/social/user-profile/user-profile').then((m) => m.UserProfile),
  },
  {
    path: 'users/:id/reviews',
    loadComponent: () => import('./features/social/user-reviews/user-reviews').then((m) => m.UserReviews),
  },
  {
    path: 'users/:id/lists',
    loadComponent: () => import('./features/social/user-lists/user-lists').then((m) => m.UserLists),
  },
  {
    path: 'discover/reviews',
    canActivate: [authGuard],
    loadComponent: () => import('./features/discover/discover-reviews/discover-reviews').then((m) => m.DiscoverReviews),
  },
  {
    path: 'discover/users',
    canActivate: [authGuard],
    loadComponent: () => import('./features/discover/discover-users/discover-users').then((m) => m.DiscoverUsers),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
  },
  {
    path: 'library',
    canActivate: [authGuard],
    loadComponent: () => import('./features/games/library/library').then((m) => m.Library),
  },
  {
    path: 'lists',
    canActivate: [authGuard],
    loadComponent: () => import('./features/lists/lists').then((m) => m.Lists),
  },
  {
    path: 'lists/:id',
    loadComponent: () => import('./features/lists/list-detail/list-detail').then((m) => m.ListDetail),
  },
  {
    path: 'admin/games',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/manage-games/manage-games').then((m) => m.ManageGames),
  },
];
