import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { ApiResponse, ProblemDetails } from '../models/api.models';

export class ApiError extends Error {
  constructor(message: string, readonly status: number, readonly code?: string, readonly activeBookingId?: string) { super(message); }
}

@Injectable({ providedIn: 'root' })
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  get<T>(path: string, params?: HttpParams) { return this.request(this.http.get<ApiResponse<T>>(`${this.baseUrl}${path}`, { params })); }
  post<T>(path: string, body: unknown = {}) { return this.request(this.http.post<ApiResponse<T>>(`${this.baseUrl}${path}`, body)); }
  put<T>(path: string, body: unknown) { return this.request(this.http.put<ApiResponse<T>>(`${this.baseUrl}${path}`, body)); }
  delete<T>(path: string) { return this.request(this.http.delete<ApiResponse<T>>(`${this.baseUrl}${path}`)); }

  private request<T>(source: Observable<ApiResponse<T>>) {
    return source.pipe(
      map(response => {
        if (!response.success) throw new ApiError(response.message ?? 'Yêu cầu không thành công.', 200);
        return response.data;
      }),
      catchError((error: HttpErrorResponse | ApiError) => {
        if (error instanceof ApiError) return throwError(() => error);
        const problem = error.error as ProblemDetails | undefined;
        const validation = problem?.errors ? Object.values(problem.errors).flat()[0] : undefined;
        return throwError(() => new ApiError(validation ?? problem?.detail ?? problem?.title ?? 'Không thể kết nối máy chủ.', error.status, problem?.code, problem?.activeBookingId));
      }),
    );
  }
}
