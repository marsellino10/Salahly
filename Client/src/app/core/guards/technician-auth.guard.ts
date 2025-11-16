import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';

export const technicianAuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const isBrowser = isPlatformBrowser(platformId);

  if (!isBrowser) {
    return true;
  }
  const token = authService.getToken();
  if (!token) {
    return router.parseUrl('/login');
  }

  const role = extractRoleFromToken(token);
  if (role?.toLowerCase() === 'craftsman') {
    return true;
  }

  return router.parseUrl('/home');
};

function extractRoleFromToken(token: string): string | null {
  const payload = decodeJwtPayload(token);
  const roleCandidate =
    payload?.['role'] ??
    payload?.['Role'] ??
    payload?.['userType'] ??
    payload?.['UserType'] ??
    null;

  return typeof roleCandidate === 'string' ? roleCandidate : null;
}

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const [, payloadSegment] = token.split('.');
    if (!payloadSegment) {
      return null;
    }

    let normalized = payloadSegment.replace(/-/g, '+').replace(/_/g, '/');
    while (normalized.length % 4 !== 0) {
      normalized += '=';
    }

    if (typeof atob !== 'function') {
      return null;
    }

    const decodedString = atob(normalized);
    return JSON.parse(decodedString);
  } catch {
    return null;
  }
}
