import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';

export const customerAuthGuard: CanActivateFn = () => {
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
    console.log('CustomerAuthGuard: User not authenticated, redirecting to login');
    return router.parseUrl('/login');
  }

  // Use AuthService method for consistency
  const userType = authService.getUserType();
  
  // Check if user is a customer (case-insensitive)
  if (userType && userType.toLowerCase() === 'customer') {
    console.log('CustomerAuthGuard: Access granted for customer');
    return true;
  }

  // User is authenticated but not a customer
  console.log(`CustomerAuthGuard: Access denied. User type: ${userType}, redirecting to home`);
  return router.parseUrl('/home');
};

