import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { AuthService } from './auth-service';

export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface CustomerResponse {
  id: number;
  fullName: string;
  profileImageUrl?: string | null;
  address?: string | null;
  city?: string | null;
  area?: string | null;
  phoneNumber?: string | null;
  dateOfBirth?: string | null;
}

export interface CustomerUpdatePayload {
  address?: string | null;
  city?: string | null;
  area?: string | null;
  phoneNumber?: string | null;
  dateOfBirth?: Date | string | null;
}

export interface CreateCustomerPayload extends CustomerUpdatePayload {
  fullName: string;
}

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly _authService = inject(AuthService);
  private readonly baseUrl = `${environment.baseApi}customer`;

  public getCustomerById(id: number): Observable<CustomerResponse> {
    return this._httpClient.get<CustomerResponse>(`${this.baseUrl}/${id}`);
  }

  public updateCustomer(
    id: number,
    payload: CustomerUpdatePayload,
    profileImage?: File | null,
  ): Observable<CustomerResponse> {
    const formData = this.buildUpdateCustomerFormData(payload, profileImage ?? undefined);
    return this._httpClient.put<CustomerResponse>(`${this.baseUrl}/${id}`, formData);
  }

  public createCustomer(
    payload: CreateCustomerPayload,
    profileImage?: File | null,
  ): Observable<ApiResponse<CustomerResponse>> {
    const formData = this.buildCreateCustomerFormData(payload, profileImage ?? undefined);
    return this._httpClient.post<ApiResponse<CustomerResponse>>(`${this.baseUrl}/create`, formData);
  }

  private buildCreateCustomerFormData(payload: CreateCustomerPayload, profileImage?: File): FormData {
    const formData = this.buildBaseCustomerFormData(payload);
    formData.append('FullName', payload.fullName);

    if (profileImage) {
      formData.append('ImageProfile', profileImage);
    }

    return formData;
  }

  private buildUpdateCustomerFormData(payload: CustomerUpdatePayload, profileImage?: File): FormData {
    const formData = this.buildBaseCustomerFormData(payload);

    if (profileImage) {
      formData.append('ImageProfile', profileImage);
    }

    return formData;
  }

  private buildBaseCustomerFormData(payload: CustomerUpdatePayload): FormData {
    const formData = new FormData();

    this.appendStringIfPresent(formData, 'Address', payload.address);
    this.appendStringIfPresent(formData, 'City', payload.city);
    this.appendStringIfPresent(formData, 'Area', payload.area);
    this.appendStringIfPresent(formData, 'PhoneNumber', payload.phoneNumber);

    const normalizedDate = this.normalizeDateValue(payload.dateOfBirth);
    if (normalizedDate) {
      formData.append('DateOfBirth', normalizedDate);
    }

    return formData;
  }

  private appendStringIfPresent(formData: FormData, key: string, value?: string | null): void {
    if (value !== undefined && value !== null) {
      formData.append(key, value);
    }
  }

  private normalizeDateValue(value?: Date | string | null): string | null {
    if (!value) {
      return null;
    }

    if (value instanceof Date) {
      return value.toISOString();
    }

    const parsed = new Date(value);
    return isNaN(parsed.getTime()) ? value : parsed.toISOString();
  }

  public getCustomerTokenClaims(): {
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
