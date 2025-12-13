import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface CreateReviewPayload {
  reviewerUserId: number;
  targetUserId: number;
  bookingId: number;
  rating: number;
  comment: string;
}

export interface ReviewDto {
  reviewerUserId: number;
  targetUserId: number;
  bookingId: number;
  rating: number;
  comment: string;
  createdAt?: string;
}

@Injectable({ providedIn: 'root' })
export class ReviewsService {
  private readonly _httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.baseApi}reviews`;

  createReview(payload: CreateReviewPayload): Observable<ApiResponse<boolean>> {
    return this._httpClient.post<ApiResponse<boolean>>(this.baseUrl, payload);
  }

  getReviewsForBooking(bookingId: number): Observable<ApiResponse<ReviewDto[]>> {
    return this._httpClient.get<ApiResponse<ReviewDto[]>>(`${this.baseUrl}/booking/${bookingId}`);
  }
}
