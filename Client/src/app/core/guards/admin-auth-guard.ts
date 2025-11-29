import { inject, PLATFORM_ID } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { isPlatformBrowser } from '@angular/common';

export const adminAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const isBrowser = isPlatformBrowser(platformId);

  // Allow access during SSR
  if (!isBrowser) {
    return true;
  }

  // Check if user is authenticated
  if (!authService.isAuthenticated()) {
    console.log('AdminAuthGuard: User not authenticated, redirecting to login');
    return router.parseUrl('/login');
  }

  // Use AuthService method for consistency
  const userType = authService.getUserType();
  
  // Check if user is a admin (case-insensitive)
  if (userType && (userType.toLowerCase() === 'admin' || userType.toLowerCase() === 'admin')) {
    console.log('AdminAuthGuard: Access granted for admin');
    return true;
  }

  // User is authenticated but not a admin
  console.log(`AdminAuthGuard: Access denied. User type: ${userType}, redirecting to home`);
  return router.parseUrl('/dashboard');
};
