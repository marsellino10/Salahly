import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Area } from '../models/Area';
import { environment } from '../environments/environment';

export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface CreateAreaPayload {
  region: string;
  city: string;
}

export interface UpdateAreaPayload extends CreateAreaPayload {
  id: number;
}

@Injectable({
  providedIn: 'root',
})
export class AreaService {
  private readonly _HttpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}Area`;

  GetAllAreas(): Observable<ApiResponse<Area[]>> {
    return this._HttpClient.get<ApiResponse<Area[]>>(this.baseUrl);
  }

  GetAreaById(id: number): Observable<ApiResponse<Area>> {
    return this._HttpClient.get<ApiResponse<Area>>(`${this.baseUrl}/${id}`);
  }

  CreateArea(payload: CreateAreaPayload): Observable<Area> {
    return this._HttpClient
      .post<ApiResponse<Area>>(this.baseUrl, payload)
      .pipe(map((response) => response.data));
  }

  UpdateArea(payload: UpdateAreaPayload): Observable<Area> {
    return this._HttpClient
      .put<ApiResponse<Area>>(`${this.baseUrl}/${payload.id}`, payload)
      .pipe(map((response) => response.data));
  }

  DeleteArea(id: number): Observable<number> {
    return this._HttpClient
      .delete<ApiResponse<{ id: number }>>(`${this.baseUrl}/${id}`)
      .pipe(map((response) => response.data?.id ?? id));
  }
}
