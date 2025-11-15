import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CraftService {

  private readonly _HttpClient: HttpClient = inject(HttpClient);

  GetAllCrafts(): Observable<any> {
    return this._HttpClient.get<any>(`${environment.baseApi}Crafts`);
  }

  GetCraftById(id: number): Observable<any> {
    return this._HttpClient.get<any>(`${environment.baseApi}Crafts/${id}`);
  }

  CreateCraft(craft: any): Observable<any> {
    return this._HttpClient.post<any>(`${environment.baseApi}Crafts`, craft);
  }

  UpdateCraft(id: number, craft: any): Observable<any> {
    return this._HttpClient.put<any>(`${environment.baseApi}Crafts/${id}`, craft);
  }

  DeleteCraft(id: number): Observable<any> {
    return this._HttpClient.delete<any>(`${environment.baseApi}Crafts/${id}`);
  }
  
}
