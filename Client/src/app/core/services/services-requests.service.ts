import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export type ServiceRequestStatus =
  | 'Open'
  | 'HasOffers'
  | 'OfferAccepted'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'
  | 'Expired';

export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface ServiceRequestDto {
  serviceRequestId: number;
  title: string;
  description: string;
  city: string | null;
  area: string | null;
  address: string;
  availableFromDate: string;
  availableToDate: string;
  customerBudget?: number | null;
  status: ServiceRequestStatus | string;
  offersCount: number;
  maxOffers: number;
  createdAt: string;
  expiresAt: string;
  craftName?: string | null;
  customerName?: string | null;
  images?: string[] | null;
  paymentMethod?: string | null;
  customerPhoneNumber?: string | null;
}

export interface ServiceRequestResponseDto extends ServiceRequestDto {
  customerId?: number;
  craftId?: number;
  latitude?: number | null;
  longitude?: number | null;
  imagesJson?: string | null;
}

export interface CreateServiceRequestPayload {
  craftId: number;
  title: string;
  description: string;
  address: string;
  areaId: number;
  availableFromDate: Date | string;
  availableToDate: Date | string;
  customerBudget?: number | null;
  latitude?: number | null;
  longitude?: number | null;
  maxOffers?: number;
  paymentMethod?: string | null;
}

export interface UpdateServiceRequestPayload {
  title?: string;
  description?: string;
  address?: string;
  areaId?: number;
  availableFromDate?: Date | string | null;
  availableToDate?: Date | string | null;
  customerBudget?: number | null;
  paymentMethod?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ServicesRequestsService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}customer/servicerequests`;

  getCustomerRequests(): Observable<ApiResponse<ServiceRequestDto[]>> {
    return this._httpClient.get<ApiResponse<ServiceRequestDto[]>>(this.baseUrl);
  }

  getRequestById(id: number): Observable<ApiResponse<ServiceRequestDto>> {
    return this._httpClient.get<ApiResponse<ServiceRequestDto>>(`${this.baseUrl}/${id}`);
  }

  createRequest(
    payload: CreateServiceRequestPayload,
    imageFiles: File[] = [],
  ): Observable<ServiceRequestResponseDto> {
    const formData = this.buildCreateFormData(payload, imageFiles);
    return this._httpClient.post<ServiceRequestResponseDto>(this.baseUrl, formData);
  }

  updateRequest(
    id: number,
    payload: UpdateServiceRequestPayload,
  ): Observable<ApiResponse<ServiceRequestDto>> {
    const formData = this.buildUpdateFormData(payload);
    return this._httpClient.put<ApiResponse<ServiceRequestDto>>(`${this.baseUrl}/${id}`, formData);
  }

  deleteRequest(id: number): Observable<void> {
    return this._httpClient.delete<void>(`${this.baseUrl}/${id}`);
  }

  private buildCreateFormData(payload: CreateServiceRequestPayload, imageFiles: File[]): FormData {
    const formData = new FormData();

    formData.append('CraftId', payload.craftId.toString());
    formData.append('Title', payload.title);
    formData.append('Description', payload.description);
    formData.append('Address', payload.address);
    formData.append('AreaId', payload.areaId.toString());
    formData.append('AvailableFromDate', this.normalizeDate(payload.availableFromDate));
    formData.append('AvailableToDate', this.normalizeDate(payload.availableToDate));
    formData.append('PaymentMethod', payload.paymentMethod ?? '');

    if (payload.customerBudget !== undefined && payload.customerBudget !== null) {
      formData.append('CustomerBudget', payload.customerBudget.toString());
    }

    if (payload.latitude !== undefined && payload.latitude !== null) {
      formData.append('Latitude', payload.latitude.toString());
    }

    if (payload.longitude !== undefined && payload.longitude !== null) {
      formData.append('Longitude', payload.longitude.toString());
    }

    if (payload.maxOffers !== undefined && payload.maxOffers !== null) {
      formData.append('MaxOffers', payload.maxOffers.toString());
    }

    imageFiles.forEach((file) => {
      formData.append('ImageFiles', file, file.name);
    });

    return formData;
  }

  private buildUpdateFormData(payload: UpdateServiceRequestPayload): FormData {
    const formData = new FormData();

    if (payload.title) {
      formData.append('Title', payload.title);
    }

    if (payload.description) {
      formData.append('Description', payload.description);
    }

    if (payload.address) {
      formData.append('Address', payload.address);
    }

    if (payload.areaId !== undefined && payload.areaId !== null) {
      formData.append('AreaId', payload.areaId.toString());
    }

    if (payload.availableFromDate) {
      formData.append('AvailableFromDate', this.normalizeDate(payload.availableFromDate));
    }

    if (payload.availableToDate) {
      formData.append('AvailableToDate', this.normalizeDate(payload.availableToDate));
    }

    if (payload.customerBudget !== undefined && payload.customerBudget !== null) {
      formData.append('CustomerBudget', payload.customerBudget.toString());
    }
    if (payload.paymentMethod) {
      formData.append('PaymentMethod', payload.paymentMethod);
    }

    return formData;
  }

  private normalizeDate(value: Date | string): string {
    if (value instanceof Date) {
      return value.toISOString();
    }

    const parsed = new Date(value);
    return isNaN(parsed.getTime()) ? value : parsed.toISOString();
  }
}