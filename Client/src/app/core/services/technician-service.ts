import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class TechnicianService {
  
  private readonly _HttpClient: HttpClient = inject(HttpClient);

  public getTechnicians(
    pageNumber: number = 1,
    pageSize: number = 10,
    SearchName: string = '',
    CraftId: number = 0,
    Region: string = '',
    City: string = '',
    IsAvailable: boolean = true,
  ): Observable<any> {
    let url = `${environment.baseApi}craftsman?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (SearchName != '') url += `&searchName=${SearchName}`;
    if (CraftId != 0) url += `&craftId=${CraftId}`;
    if (Region != '') url += `&region=${Region}`;
    if (City != '') url += `&city=${City}`;
    if (IsAvailable) url += `&isAvailable=${IsAvailable}`;
    return this._HttpClient.get<any>(url);
  }
}
