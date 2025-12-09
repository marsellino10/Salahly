import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export enum OfferStatus {
  Pending = 0,
  Accepted = 1,
  Rejected = 2,
  Withdrawn = 3,
  Expired = 4,
}

export interface OfferDto {
  craftsmanOfferId: number;
  craftsmanId:number | null;
  craftsmanProfileImageUrl: string | null;
  offeredPrice: number;
  description: string;
  estimatedDurationMinutes: number;
  preferredDate: string;
  preferredTimeSlot?: string | null;
  craftsmanName: string;
  status: OfferStatus;
  createdAt: string;
  rejectionReason?: string | null;
}

export interface RejectOfferPayload {
  rejectionReason?: string | null;
}

export interface ServiceResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({ providedIn: 'root' })
export class CustomerOffersService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}customer`;

  getOffersForRequest(
    requestId: number,
  ): Observable<ServiceResponse<OfferDto[]>> {
    const url = `${this.baseUrl}/service-requests/${requestId}/offers`;
    return this._httpClient.get<ServiceResponse<OfferDto[]>>(url);
  }

  acceptOffer(offerId: number): Observable<ServiceResponse<boolean>> {
    const url = `${this.baseUrl}/accept/${offerId}`;
    return this._httpClient.post<ServiceResponse<boolean>>(url, {});
  }

  rejectOffer(
    offerId: number,
    payload: RejectOfferPayload,
  ): Observable<ServiceResponse<boolean>> {
    const url = `${this.baseUrl}/offers/${offerId}/reject`;
    return this._httpClient.patch<ServiceResponse<boolean>>(url, payload);
  }
}
