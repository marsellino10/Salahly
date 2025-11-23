import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { ServiceRequestDto } from './services-requests.service';

export interface ServiceResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({ providedIn: 'root' })
export class CraftsmanServiceRequestService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}craftsman/service-requests`;

  getAvailableOpportunities(): Observable<ServiceResponse<ServiceRequestDto[]>> {
    const url = `${this.baseUrl}/opportunities`;
    return this._httpClient.get<ServiceResponse<ServiceRequestDto[]>>(url);
  }

  getRequestsCraftsmanOfferedOn(): Observable<ServiceResponse<ServiceRequestDto[]>> {
    const url = `${this.baseUrl}/offers`;
    return this._httpClient.get<ServiceResponse<ServiceRequestDto[]>>(url);
  }

  getServiceRequestById(
    requestId: number,
  ): Observable<ServiceResponse<ServiceRequestDto>> {
    const url = `${this.baseUrl}/${requestId}`;
    return this._httpClient.get<ServiceResponse<ServiceRequestDto>>(url);
  }
}
