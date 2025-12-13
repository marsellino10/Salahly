import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { ServiceRequestDto } from './services-requests.service';

export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface BookingDto {
  bookingId: number;
  customerId: number;
  craftsmanId: number;
  craftId: number;
  bookingDate: string;
  paymentDeadline?: string | null;
  status: string;
  totalAmount: number;
  notes?: string | null;
  cancellationReason?: string | null;
  createdAt?: string;
}

export interface BookingWithServiceRequestDto {
  booking: BookingDto;
  serviceRequest: ServiceRequestDto;
  customerPhone?: string | null;
  craftsmenCount: number;
}

export interface CancellationResultDto {
  bookingId: number;
  cancellationDate: string;
  refundAmount: number;
  refundPercentage: number;
  message?: string | null;
}

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}booking`;

  getUserBookings(): Observable<ApiResponse<BookingWithServiceRequestDto[]>> {
    return this._httpClient.get<ApiResponse<BookingWithServiceRequestDto[]>>(this.baseUrl);
  }

  cancelBooking(bookingId: number, reason?: string | null): Observable<ApiResponse<CancellationResultDto>> {
    const url = `${this.baseUrl}/${bookingId}/cancel`;
    const payload = reason ? { reason } : {};
    return this._httpClient.post<ApiResponse<CancellationResultDto>>(url, payload);
  }
}
