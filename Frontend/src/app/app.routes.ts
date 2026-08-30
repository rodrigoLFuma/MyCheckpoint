import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

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
];
