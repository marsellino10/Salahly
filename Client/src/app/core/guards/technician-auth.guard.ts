import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';

export const technicianAuthGuard: CanActivateFn = () => {
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
    console.log('TechnicianAuthGuard: User not authenticated, redirecting to login');
    return router.parseUrl('/login');
  }

  // Use AuthService method for consistency
  const userType = authService.getUserType();
  
  // Check if user is a technician/craftsman (case-insensitive)
  if (userType && (userType.toLowerCase() === 'craftsman' || userType.toLowerCase() === 'technician')) {
    console.log('TechnicianAuthGuard: Access granted for technician/craftsman');
    return true;
  }

  // User is authenticated but not a technician
  console.log(`TechnicianAuthGuard: Access denied. User type: ${userType}, redirecting to home`);
  return router.parseUrl('/home');
};

