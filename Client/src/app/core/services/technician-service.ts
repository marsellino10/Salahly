import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class TechnicianService {
  
  private readonly _HttpClient: HttpClient = inject(HttpClient);

  public getTechnicians(pageNumber: number = 1, pageSize: number = 10): Observable<any> {
    return this._HttpClient.get<any>(`${environment.baseApi}craftsman?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }
}
