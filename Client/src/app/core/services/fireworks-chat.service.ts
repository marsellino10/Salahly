import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';
import { environment } from '../environments/environment';

const FALLBACK_MESSAGE =
  "Thanks for asking! I’m not sure about that yet, but our support team would love to help if you reach out through the in-app support form.";

interface ChatAskRequest {
  question: string;
  context?: string;
}

interface ChatAskResponse {
  statusCode: number;
  message: string;
  data?: {
    answer: string;
    isFallback: boolean;
  };
}

export interface ChatReply {
  answer: string;
  isFallback: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class FireworksChatService {
  private readonly _http = inject(HttpClient);
  private readonly endpoint = `${environment.baseApi}chat/ask`;

  sendMessage(question: string, context?: string): Observable<ChatReply> {
    const payload: ChatAskRequest = {
      question: question.trim(),
      context: context?.trim() || undefined,
    };

    return this._http.post<ChatAskResponse>(this.endpoint, payload).pipe(
      map((response) => {
        const answer = response.data?.answer?.trim() || FALLBACK_MESSAGE;
        const isFallback = response.data?.isFallback ?? answer === FALLBACK_MESSAGE;
        return { answer, isFallback } satisfies ChatReply;
      }),
      catchError((error: HttpErrorResponse) => {
        const message =
          typeof error.error === 'string'
            ? error.error
            : error.error?.message ??
              error.statusText ??
              'Unable to reach the support assistant at the moment. Please try again later.';
        return throwError(() => new Error(message));
      })
    );
  }
}
