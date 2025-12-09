import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export enum CraftsmanOfferStatus {
  Pending = 0,
  Accepted = 1,
  Rejected = 2,
  Withdrawn = 3,
  Expired = 4,
}

export interface CraftsmanOfferDto {
  serviceRequestId: number;
  craftsmanOfferId: number;
  offeredPrice: number;
  description: string;
  estimatedDurationMinutes: number;
  preferredDate: string;
  preferredTimeSlot?: string | null;
  craftsmanName: string;
  status: CraftsmanOfferStatus | string;
  createdAt: string;
  rejectionReason?: string | null;
}

export interface CreateCraftsmanOfferPayload {
  serviceRequestId: number;
  offeredPrice: number;
  description: string;
  estimatedDurationMinutes: number;
  preferredDate: string; 
  preferredTimeSlot?: string;
}

export interface ServiceResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({ providedIn: 'root' })
export class CraftsmanOffersService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}craftsman/offers`;

  createOffer(payload: CreateCraftsmanOfferPayload): Observable<ServiceResponse<CraftsmanOfferDto>> {
    return this._httpClient.post<ServiceResponse<CraftsmanOfferDto>>(this.baseUrl, payload);
  }

  getMyOffers(): Observable<ServiceResponse<CraftsmanOfferDto[]>> {
    return this._httpClient.get<ServiceResponse<CraftsmanOfferDto[]>>(this.baseUrl);
  }

  getOfferById(offerId: number): Observable<ServiceResponse<CraftsmanOfferDto>> {
    const url = `${this.baseUrl}/${offerId}`;
    return this._httpClient.get<ServiceResponse<CraftsmanOfferDto>>(url);
  }
  getOfferByServiceRequestId(serviceRequestId: number): Observable<ServiceResponse<CraftsmanOfferDto>> {
    const url = `${this.baseUrl}/${serviceRequestId}/offer`;
    return this._httpClient.get<ServiceResponse<CraftsmanOfferDto>>(url);
  }

  withdrawOffer(offerId: number): Observable<ServiceResponse<boolean>> {
    const url = `${this.baseUrl}/${offerId}/withdraw`;
    return this._httpClient.patch<ServiceResponse<boolean>>(url, {});
  }
}
