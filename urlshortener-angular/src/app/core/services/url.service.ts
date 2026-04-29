import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ShortenedUrl } from '../../models/url.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class UrlService {
  private apiUrl = 'https://localhost:7222/api/urls';

  constructor(private http: HttpClient, private auth: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.auth.getToken()}`
    });
  }

  getAll(): Observable<ShortenedUrl[]> {
    return this.http.get<ShortenedUrl[]>(this.apiUrl);
  }

  create(originalUrl: string): Observable<ShortenedUrl> {
    return this.http.post<ShortenedUrl>(
      this.apiUrl,
      { originalUrl },
      { headers: this.getHeaders() }
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() }
    );
  }
}