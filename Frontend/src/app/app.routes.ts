import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {

    path: '',
    loadComponent: () => import('./features/home/home').then((m) => m.Home),
  },
  { path: 'login', loadComponent: () => import('./features/auth/login/login').then((m) => m.Login) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register').then((m) => m.Register) },
  {
    path: 'games/search',
    canActivate: [authGuard],
    loadComponent: () => import('./features/games/search/game-search').then((m) => m.GameSearch),
  },
  {
    path: 'social',
    canActivate: [authGuard],
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
    path: 'users/:id/followers',
    loadComponent: () => import('./features/social/user-followers/user-followers').then((m) => m.UserFollowers),
  },
  {
    path: 'users/:id/following',
    loadComponent: () => import('./features/social/user-following/user-following').then((m) => m.UserFollowing),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    loadComponent: () => import('./features/settings/settings').then((m) => m.Settings),
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

    path: 'lists/discover',
    canActivate: [authGuard],
    loadComponent: () => import('./features/lists/discover-lists/discover-lists').then((m) => m.DiscoverLists),
  },
  {
    path: 'lists/:id',
    loadComponent: () => import('./features/lists/list-detail/list-detail').then((m) => m.ListDetail),
  },
  {
    path: 'rankings',
    canActivate: [authGuard],
    loadComponent: () => import('./features/rankings/rankings').then((m) => m.Rankings),
  },
  {
    path: 'notifications',
    canActivate: [authGuard],
    loadComponent: () => import('./features/notifications/notifications').then((m) => m.Notifications),
  },
  {
    path: 'admin/games',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/manage-games/manage-games').then((m) => m.ManageGames),
  },
  {
    path: 'admin/categories',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./features/admin/manage-categories/manage-categories').then((m) => m.ManageCategories),
  },
  {
    path: 'admin/users',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/manage-users/manage-users').then((m) => m.ManageUsers),
  },
];
