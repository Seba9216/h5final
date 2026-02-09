import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isLoggedIn = authService.isLoggedIn();
  console.log('Auth Guard - isLoggedIn:', isLoggedIn);
  console.log('Auth Guard - route:', state.url);

  if (isLoggedIn) {
    return true;
  }

  console.log('Auth Guard - Redirecting to /login');
  router.navigate(['/login']);
  return false;
};
