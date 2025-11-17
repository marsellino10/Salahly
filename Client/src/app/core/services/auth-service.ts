import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { inject, Injectable, PLATFORM_ID } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';

export interface LoginPayload {
  userName: string;
  password: string;
}

export interface LoginResponse {
  statusCode:number,
  message:string,
  data: {
    token: string;
    userType: string;
    isProfileCompleted?: boolean | null;
  }
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

  login(payload: LoginPayload): Observable<LoginResponse> {
    return this._httpClient
      .post<LoginResponse>(`${this.baseUrl}/login`, payload)
      .pipe(tap((result) => this.storeToken(result.data.token)));
  }

  registerCustomer(payload: RegisterPayload): Observable<any> {
    return this._httpClient.post<any>(`${this.baseUrl}/register-customer`, payload);
  }

  registerTechnician(payload: RegisterPayload): Observable<any> {
    return this._httpClient.post<any>(`${this.baseUrl}/register-technician`, payload);
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

  getToken(): string | null {
    if (!this.canUseStorage()) {
      return null;
    }
    return localStorage.getItem(this.tokenStorageKey);
  }

  clearToken(): void {
    if (!this.canUseStorage()) {
      return;
    }

    localStorage.removeItem(this.tokenStorageKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getUserType(): string | null {
    return this.getDecodedPayloadField('userType') ?? this.getDecodedPayloadField('role');
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
  }  
}

function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}
