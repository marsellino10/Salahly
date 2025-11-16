import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';
import { Craftsman } from '../models/Craftman';
import { AuthService } from './auth-service';

export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
  pageNumber: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface TechnicianServiceAreaPayload {
  areaId: number;
  serviceRadiusKm?: number;
}

export interface CreateTechnicianPayload {
  fullName: string;
  craftId: number;
  hourlyRate?: number | null;
  bio?: string | null;
  yearsOfExperience?: number | null;
  serviceAreas?: TechnicianServiceAreaPayload[];
}

export interface UpdateTechnicianPayload extends CreateTechnicianPayload {
  id: number;
}

@Injectable({
  providedIn: 'root',
})
export class TechnicianService {
  private readonly baseUrl = `${environment.baseApi}Craftsman`;
  private readonly _HttpClient: HttpClient = inject(HttpClient);
  private readonly _authService = inject(AuthService);

  public getTechnicians(
    pageNumber: number = 1,
    pageSize: number = 10,
    SearchName: string = '',
    CraftId: number = 0,
    Region: string = '',
    City: string = '',
    IsAvailable: boolean | null = true,
    MinRating?: number,
    MaxHourlyRate?: number,
  ): Observable<ApiResponse<PaginatedResponse<Craftsman>>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (SearchName.trim()) params = params.set('searchName', SearchName.trim());
    if (CraftId) params = params.set('craftId', CraftId);
    if (Region.trim()) params = params.set('region', Region.trim());
    if (City.trim()) params = params.set('city', City.trim());
    if (IsAvailable !== null && IsAvailable !== undefined)
      params = params.set('isAvailable', IsAvailable);
    if (MinRating !== undefined)
      params = params.set('minRating', MinRating);
    if (MaxHourlyRate !== undefined)
      params = params.set('maxHourlyRate', MaxHourlyRate);

    return this._HttpClient.get<ApiResponse<PaginatedResponse<Craftsman>>>(this.baseUrl, { params });
  }

  public getTechnicianById(id: number): Observable<ApiResponse<Craftsman>> {
    return this._HttpClient.get<ApiResponse<Craftsman>>(`${this.baseUrl}/${id}`);
  }

  public createTechnician(
    payload: CreateTechnicianPayload,
    profileImage?: File | null,
  ): Observable<ApiResponse<Craftsman>> {
    const formData = this.buildCraftsmanFormData(payload, profileImage ?? undefined);
    return this._HttpClient.post<ApiResponse<Craftsman>>(this.baseUrl, formData);
  }

  public updateTechnician(
    payload: UpdateTechnicianPayload,
    profileImage?: File | null,
  ): Observable<ApiResponse<Craftsman>> {
    const formData = this.buildCraftsmanFormData(payload, profileImage ?? undefined, true);
    return this._HttpClient.put<ApiResponse<Craftsman>>(`${this.baseUrl}/${payload.id}`, formData);
  }

  public deleteTechnician(id: number): Observable<ApiResponse<{ id: number }>> {
    return this._HttpClient.delete<ApiResponse<{ id: number }>>(`${this.baseUrl}/${id}`);
  }

  public getTechnicianTokenClaims(): {
    name: string | null;
    nameIdentifier: string | null;
    role: string | null;
  } {
    const token = this._authService.getToken();
    if (!token) {
      return { name: null, nameIdentifier: null, role: null };
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
    const role = this.extractClaim(payload, ['role', 'Role', 'userType', 'UserType']);

    return { name, nameIdentifier, role };
  }

  private buildCraftsmanFormData(
    payload: CreateTechnicianPayload | UpdateTechnicianPayload,
    profileImage?: File,
    includeId: boolean = false,
  ): FormData {
    const formData = new FormData();

    if (includeId) {
      formData.append('Id', `${(payload as UpdateTechnicianPayload).id}`);
    }

    formData.append('FullName', payload.fullName);
    formData.append('CraftId', payload.craftId.toString());

    if (payload.hourlyRate !== undefined && payload.hourlyRate !== null) {
      formData.append('HourlyRate', payload.hourlyRate.toString());
    }

    if (payload.bio) {
      formData.append('Bio', payload.bio);
    }

    if (payload.yearsOfExperience !== undefined && payload.yearsOfExperience !== null) {
      formData.append('YearsOfExperience', payload.yearsOfExperience.toString());
    }

    if (payload.serviceAreas?.length) {
      const serviceAreasJson = JSON.stringify(
        payload.serviceAreas.map((area) => ({
          areaId: area.areaId,
          serviceRadiusKm: area.serviceRadiusKm ?? 10,
        })),
      );
      formData.append('serviceAreasJson', serviceAreasJson);
    }

    if (profileImage) {
      formData.append('profileImage', profileImage);
    }

    return formData;
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
