import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { inject, Injectable, PLATFORM_ID } from '@angular/core';
import { map, Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';

export interface LoginPayload {
  userName: string;
  password: string;
}

export interface AuthTokens {
  token: string;
  refreshToken: string;
}

export interface LoginData extends AuthTokens {
  userType: string;
  isProfileCompleted?: boolean | null;
}

export interface LoginResponse {
  statusCode: number;
  message: string;
  data: LoginData;
}

export interface RefreshResponse {
  statusCode: number;
  message: string;
  data: AuthTokens;
}

export interface RegisterPayload {
  fullName: string;
  userName: string;
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly _platformId = inject(PLATFORM_ID);

  private readonly baseUrl = `${environment.baseApi}Auth`;
  private readonly tokenStorageKey = 'salahly_auth_token';
  private readonly refreshTokenStorageKey = 'salahly_refresh_token';

  login(payload: LoginPayload): Observable<LoginResponse> {
    return this._httpClient
      .post<LoginResponse>(`${this.baseUrl}/login`, payload)
      .pipe(tap((result) => this.storeTokens(result.data.token, result.data.refreshToken)));
  }

  refresh(refreshToken: string): Observable<AuthTokens> {
    return this._httpClient
      .post<RefreshResponse>(`${this.baseUrl}/refresh`, {refreshToken})
      .pipe(
        map((response) => response.data),
        tap((tokens) => this.storeTokens(tokens?.token ?? null, tokens?.refreshToken ?? null))
      );
  }

  registerCustomer(payload: RegisterPayload): Observable<any> {
    return this._httpClient.post<any>(`${this.baseUrl}/register-customer`, payload);
  }

  registerTechnician(payload: RegisterPayload): Observable<any> {
    return this._httpClient.post<any>(`${this.baseUrl}/register-technician`, payload);
  }

  storeTokens(accessToken: string | null | undefined, refreshToken: string | null | undefined): void {
    this.storeToken(accessToken ?? null);
    this.storeRefreshToken(refreshToken ?? null);
  }

  storeToken(token: string | null): void {
    if (!this.canUseStorage()) {
      return;
    }

    if (!token) {
      this.clearToken();
      return;
    }

    localStorage.setItem(this.tokenStorageKey, token);
  }

  storeRefreshToken(token: string | null): void {
    if (!this.canUseStorage()) {
      return;
    }

    if (!token) {
      this.clearRefreshToken();
      return;
    }

    localStorage.setItem(this.refreshTokenStorageKey, token);
  }

  getToken(): string | null {
    if (!this.canUseStorage()) {
      return null;
    }
    return localStorage.getItem(this.tokenStorageKey);
  }

  getRefreshToken(): string | null {
    if (!this.canUseStorage()) {
      return null;
    }
    return localStorage.getItem(this.refreshTokenStorageKey);
  }

  clearToken(): void {
    if (!this.canUseStorage()) {
      return;
    }

    localStorage.removeItem(this.tokenStorageKey);
  }

  clearRefreshToken(): void {
    if (!this.canUseStorage()) {
      return;
    }

    localStorage.removeItem(this.refreshTokenStorageKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getUserType(): string | null {
    return this.getDecodedPayloadField('http://schemas.microsoft.com/ws/2008/06/identity/claims/role') ?? 
           this.getDecodedPayloadField('http://schemas.microsoft.com/ws/2008/06/identity/claims/userType') ??
           this.getDecodedPayloadField('role') ?? 
           this.getDecodedPayloadField('userType');
  }

  private getDecodedPayloadField(field: string): string | null {
    const payload = this.decodeTokenPayload();
    const value = payload?.[field] ?? payload?.[capitalize(field)] ?? null;
    return typeof value === 'string' ? value : null;
  }

  private decodeTokenPayload(): Record<string, unknown> | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

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

  private canUseStorage(): boolean {
    return isPlatformBrowser(this._platformId);
  }

  logout(): void {
    this.clearToken();
    this.clearRefreshToken();
  }  
}

function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}
