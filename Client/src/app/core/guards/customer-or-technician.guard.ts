import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';

export const customerOrTechnicianGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const isBrowser = isPlatformBrowser(platformId);

  if (!isBrowser) {
    return true;
  }

  if (!authService.isAuthenticated()) {
    return router.parseUrl('/login');
  }

  const userType = authService.getUserType()?.toLowerCase();

  if (userType && (userType === 'customer' || userType === 'technician' || userType === 'craftsman')) {
    return true;
  }

  return router.parseUrl('/home');
};
