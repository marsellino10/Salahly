import { PortfolioItemsService } from './portfolio-items.service';
import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../environments/environment';
import { AuthService } from './auth-service';
import { ServiceRequestDto } from './services-requests.service';


export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface BookingAdminViewDto {
  bookingId: number;
  bookingDate: string;
  totalAmount: number;
}

export interface CraftsManAdminViewDto {
  id: number;
  fullName: string;
  isVerified: boolean;
}

export interface AreaStatsDto {
  areaId: number;
  region: string;
  city: string;
  requestCount: number;
}

export interface OffersStatsDto {
  totalOffers: number;
  averageOffersPerServiceRequest: number;
}

export interface CraftsmanShortDto {
  id: number;
  fullName: string;
  craftName?: string | null;
  ratingAverage: number;
}

export interface CraftAverageReviewDto {
  craftId: number;
  craftName: string;
  averageReview: number;
}

export interface AdminServiceRequestFilters {
  from?: Date | string | null;
  to?: Date | string | null;
  craftId?: number | null;
  areaId?: number | null;
  orderBy?: 'date' | 'craft' | 'area';
  asc?: boolean;
}

export interface AdminCraftsmanFilters {
  craftId?: number | null;
  areaId?: number | null;
}

export interface PortfolioItemResponseDto {
  id: number;
  craftsmanId: number;
  title: string;
  description: string;
  imageUrl: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: Date;
}

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly _authService = inject(AuthService);
  private readonly baseUrl = `${environment.baseApi}Admin`;

  // ===== Admin endpoints =====

  public getAllStatics(): Observable<BookingAdminViewDto[]> {
    return this._httpClient.get<BookingAdminViewDto[]>(this.baseUrl);
  }

  public getAllCraftsmen(): Observable<CraftsManAdminViewDto[]> {
    return this._httpClient.get<CraftsManAdminViewDto[]>(`${this.baseUrl}/craftsmen`);
  }

  public countServiceRequests(filters?: AdminServiceRequestFilters): Observable<number> {
    const params = this.buildRequestFiltersParams(filters);
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<number>>(`${this.baseUrl}/stats/service-requests/count`, { params }),
    );
  }

  public getServiceRequests(filters?: AdminServiceRequestFilters): Observable<ServiceRequestDto[]> {
    const params = this.buildRequestFiltersParams(filters);
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<ServiceRequestDto[]>>(`${this.baseUrl}/stats/service-requests`, { params }),
    );
  }

  public getMostActiveArea(): Observable<AreaStatsDto | null> {
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<AreaStatsDto | null>>(
        `${this.baseUrl}/stats/service-requests/most-active-area`,
      ),
    );
  }

  public getOffersStats(): Observable<OffersStatsDto> {
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<OffersStatsDto>>(`${this.baseUrl}/stats/offers`),
    );
  }

  public countCraftsmen(filters?: AdminCraftsmanFilters): Observable<number> {
    const params = this.buildCraftsmanParams(filters);
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<number>>(`${this.baseUrl}/stats/craftsmen/count`, { params }),
    );
  }

  public getTotalCraftsmenExperience(filters?: AdminCraftsmanFilters): Observable<number> {
    const params = this.buildCraftsmanParams(filters);
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<number>>(`${this.baseUrl}/stats/craftsmen/experience`, { params }),
    );
  }

  public getTopCraftsmenByReviews(top: number = 5): Observable<CraftsmanShortDto[]> {
    const params = new HttpParams().set('top', top.toString());
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<CraftsmanShortDto[]>>(`${this.baseUrl}/stats/craftsmen/top`, { params }),
    );
  }

  public countCrafts(): Observable<number> {
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<number>>(`${this.baseUrl}/stats/crafts/count`),
    );
  }

  public getCraftsAverageReviews(): Observable<CraftAverageReviewDto[]> {
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<CraftAverageReviewDto[]>>(
        `${this.baseUrl}/stats/crafts/average-reviews`,
      ),
    );
  }

  public getInactivePortfolio(): Observable<PortfolioItemResponseDto[]> {
    return this.unwrapResponse(
      this._httpClient.get<ApiResponse<PortfolioItemResponseDto[]>>(
        `${this.baseUrl}/portfolio/inactive`,
      ),
    );
  }
  public approvePortfolioItem(id: number): Observable<{ id: number }> {
    return this.unwrapResponse(
      this._httpClient.post<ApiResponse<{ id: number }>>(
        `${this.baseUrl}/portfolio/${id}/approve`,
        {},
      ),
    );
  }

  private unwrapResponse<T>(request$: Observable<ApiResponse<T>>): Observable<T> {
    return request$.pipe(map((response) => response.data));
  }

  private buildRequestFiltersParams(filters?: AdminServiceRequestFilters): HttpParams {
    let params = new HttpParams();

    if (!filters) {
      return params;
    }

    if (filters.from) {
      params = params.set('from', this.normalizeDate(filters.from));
    }

    if (filters.to) {
      params = params.set('to', this.normalizeDate(filters.to));
    }

    if (filters.craftId !== undefined && filters.craftId !== null) {
      params = params.set('craftId', filters.craftId.toString());
    }

    if (filters.areaId !== undefined && filters.areaId !== null) {
      params = params.set('areaId', filters.areaId.toString());
    }

    if (filters.orderBy) {
      params = params.set('orderBy', filters.orderBy);
    }

    if (typeof filters.asc === 'boolean') {
      params = params.set('asc', filters.asc ? 'true' : 'false');
    }

    return params;
  }

  private buildCraftsmanParams(filters?: AdminCraftsmanFilters): HttpParams {
    let params = new HttpParams();

    if (!filters) {
      return params;
    }

    if (filters.craftId !== undefined && filters.craftId !== null) {
      params = params.set('craftId', filters.craftId.toString());
    }

    if (filters.areaId !== undefined && filters.areaId !== null) {
      params = params.set('areaId', filters.areaId.toString());
    }

    return params;
  }

  private normalizeDate(value: Date | string): string {
    if (value instanceof Date) {
      return value.toISOString();
    }

    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? value : parsed.toISOString();
  }

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
