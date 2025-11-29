import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Portfolio } from '../models/Portfolio';
import { ApiResponse } from './technician-service';
import { environment } from '../environments/environment';

export interface CreatePortfolioItemPayload {
  title: string;
  description?: string | null;
  displayOrder?: number | null;
  image: File;
}

export interface UpdatePortfolioItemPayload {
  title: string;
  description?: string | null;
  displayOrder?: number | null;
  isActive?: boolean;
  image?: File | null;
}

@Injectable({
  providedIn: 'root',
})
export class PortfolioItemsService {
  private readonly _http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}PortfolioItems`;

  getByCraftsman(craftsmanId: number): Observable<ApiResponse<Portfolio[]>> {
    return this._http.get<ApiResponse<Portfolio[]>>(`${this.baseUrl}/craftsman/${craftsmanId}`);
  }

  getItemById(portfolioId: number): Observable<ApiResponse<Portfolio>> {
    return this._http.get<ApiResponse<Portfolio>>(`${this.baseUrl}/${portfolioId}`);
  }

  createPortfolioItem(
    craftsmanId: number,
    payload: CreatePortfolioItemPayload,
  ): Observable<ApiResponse<Portfolio>> {
    const formData = new FormData();
    formData.append('craftsmanId', `${craftsmanId}`);
    formData.append('title', payload.title.trim());
    formData.append('displayOrder', `${payload.displayOrder ?? 0}`);
    if (payload.description?.trim()) {
      formData.append('description', payload.description.trim());
    }
    formData.append('image', payload.image);

    return this._http.post<ApiResponse<Portfolio>>(this.baseUrl, formData);
  }

  updatePortfolioItem(
    portfolioId: number,
    payload: UpdatePortfolioItemPayload,
  ): Observable<ApiResponse<Portfolio>> {
    const formData = new FormData();
    formData.append('title', payload.title.trim());
    formData.append('displayOrder', `${payload.displayOrder ?? 0}`);
    formData.append('isActive', `${payload.isActive ?? true}`);
    formData.append('description', payload.description?.trim() ?? '');

    if (payload.image) {
      formData.append('image', payload.image);
    }

    return this._http.put<ApiResponse<Portfolio>>(`${this.baseUrl}/${portfolioId}`, formData);
  }

  deletePortfolioItem(portfolioId: number): Observable<ApiResponse<object>> {
    return this._http.delete<ApiResponse<object>>(`${this.baseUrl}/${portfolioId}`);
  }
}
