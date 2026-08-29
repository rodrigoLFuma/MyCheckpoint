import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Solo nos importa el 401 si ANTES estábamos logueados: la propia
      // comprobación de sesión al arrancar la app (checkSession) también
      // recibe 401 cuando no hay sesión, y eso es normal, no un problema.
      if (error.status === 401 && authService.isLoggedIn()) {
        authService.clearLocalSession();
        router.navigate(['/login']);
      }

      return throwError(() => error);
    })
  );
};
