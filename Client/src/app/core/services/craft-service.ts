import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../environments/environment';
import { Craft } from '../models/Craft';

export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface CreateCraftPayload {
  name: string;
  nameAr: string;
  description?: string | null;
  displayOrder: number;
  isActive: boolean;
}

export interface UpdateCraftPayload extends CreateCraftPayload {
  id: number;
}

@Injectable({
  providedIn: 'root',
})
export class CraftService {

  private readonly _HttpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}Crafts`;

  GetAllCrafts(isActiveOnly: boolean = false): Observable<ApiResponse<Craft[]>> {
    let params = new HttpParams();
    if (isActiveOnly) {
      params = params.set('isActiveOnly', 'true');
    }
    return this._HttpClient
      .get<ApiResponse<Craft[]>>(this.baseUrl, { params })
  }

  GetCraftById(id: number): Observable<Craft> {
    return this._HttpClient
      .get<ApiResponse<Craft>>(`${this.baseUrl}/${id}`)
      .pipe(map((response) => response.data as Craft));
  }

  CreateCraft(payload: CreateCraftPayload, iconFile?: File | null): Observable<Craft> {
    const formData = this.buildCreateFormData(payload, iconFile ?? null);
    return this._HttpClient
      .post<ApiResponse<Craft>>(this.baseUrl, formData)
      .pipe(map((response) => response.data as Craft));
  }

  UpdateCraft(payload: UpdateCraftPayload, iconFile?: File | null): Observable<Craft> {
    const formData = this.buildUpdateFormData(payload, iconFile ?? null);
    return this._HttpClient
      .put<ApiResponse<Craft>>(`${this.baseUrl}/${payload.id}`, formData)
      .pipe(map((response) => response.data as Craft));
  }

  DeleteCraft(id: number): Observable<number> {
    return this._HttpClient
      .delete<ApiResponse<{ id: number }>>(`${this.baseUrl}/${id}`)
      .pipe(map((response) => response.data?.id ?? id));
  }
  
  private buildCreateFormData(payload: CreateCraftPayload, iconFile: File | null): FormData {
    const formData = new FormData();
    formData.append('Name', payload.name.trim());
    formData.append('NameAr', payload.nameAr.trim());

    if (payload.description && payload.description.trim()) {
      formData.append('Description', payload.description.trim());
    }

    formData.append('DisplayOrder', String(payload.displayOrder ?? 0));
    formData.append('IsActive', payload.isActive ? 'true' : 'false');

    if (iconFile) {
      formData.append('iconFile', iconFile);
    }

    return formData;
  }

  private buildUpdateFormData(payload: UpdateCraftPayload, iconFile: File | null): FormData {
    const formData = this.buildCreateFormData(payload, iconFile);
    formData.append('Id', String(payload.id));
    return formData;
  }
  
}
