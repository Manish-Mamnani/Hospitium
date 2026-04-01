import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly BASE_URL = 'http://localhost:5000';

  constructor(private http: HttpClient) {}

  get<T>(endpoint: string, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        const value = params[key];
        // Only append param if it has a real value (not null, undefined, or empty string)
        if (value !== null && value !== undefined && value !== '') {
          httpParams = httpParams.set(key, value);
        }
      });
    }
    return this.http.get<T>(`${this.BASE_URL}${endpoint}`, { params: httpParams });
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<T>(`${this.BASE_URL}${endpoint}`, body);
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.BASE_URL}${endpoint}`, body);
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.BASE_URL}${endpoint}`);
  }
}
