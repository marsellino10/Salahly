import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { AuthService } from './auth-service';


export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly _authService = inject(AuthService);
  private readonly baseUrl = `${environment.baseApi}`;

  public getAdminTokenClaims(): {
    name: string | null;
    nameIdentifier: string | null;
    role: string | null;
    fullName: string | null;
  } {
    const token = this._authService.getToken();
    if (!token) {
      return { name: null, nameIdentifier: null, role: null, fullName: null };
    }

    const payload = this.decodeJwtPayload(token);
    const name = this.extractClaim(payload, ['name', 'Name', 'fullName', 'FullName', 'unique_name']);
    const nameIdentifier = this.extractClaim(payload, [
      'nameidentifier',
      'NameIdentifier',
      'nameid',
      'sub',
      'Id',
    ]);
    const role = this.extractClaim(payload, [
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/Role',
      'role',
      'Role',
    ]);
    const fullName = this.extractClaim(payload, ['fullName', 'FullName', 'unique_name']);
    return { name, nameIdentifier, role, fullName };
  }

  private extractClaim(
    payload: Record<string, unknown> | null,
    keys: string[],
  ): string | null {
    if (!payload) {
      return null;
    }

    for (const key of keys) {
      const value = payload[key] ?? payload[key.charAt(0).toUpperCase() + key.slice(1)];
      if (typeof value === 'string' && value.trim()) {
        return value;
      }
    }
    return null;
  }

  private decodeJwtPayload(token: string): Record<string, unknown> | null {
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
  
}
