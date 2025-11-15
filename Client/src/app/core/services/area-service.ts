import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AreaService {
  private _HttpClient: HttpClient = inject(HttpClient);

  GetAllAreas(): Observable<any> {
    return this._HttpClient.get<any>(`${environment.baseApi}Area`);
  }

  GetAreaById(id: number): Observable<any> {
    return this._HttpClient.get<any>(`${environment.baseApi}Area/${id}`);
  }

  UpdateArea(area: any): Observable<any> {
    return this._HttpClient.put<any>(`${environment.baseApi}Area/${area.id}`, area);
  }

  DeleteArea(id: number): Observable<any> {
    return this._HttpClient.delete<any>(`${environment.baseApi}Area/${id}`);
  }
  
}
