import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  // Se guarda a dónde intentaba ir para volver ahí justo después de
  // iniciar sesión, en vez de dejarlo siempre en el inicio.
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
