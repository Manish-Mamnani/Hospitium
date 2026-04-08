import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { TokenService } from './token.service';
import { ApiService } from './api.service';
import { ToastService } from './toast.service';
import { Observable, tap, catchError, throwError, BehaviorSubject } from 'rxjs';

interface AuthResponse {
  token: string;
}

export interface UserInfo {
  isAuthenticated: boolean;
  fullName: string | null;
  role: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private authSubject!: BehaviorSubject<UserInfo>;
  authState$!: Observable<UserInfo>;

  constructor(
    private tokenService: TokenService,
    private apiService: ApiService,
    private router: Router,
    private toastService: ToastService
  ) {
    this.authSubject = new BehaviorSubject<UserInfo>({
      isAuthenticated: this.isAuthenticated(),
      fullName: this.getFullName(),
      role: this.getRole()
    });
    this.authState$ = this.authSubject.asObservable();
  }

  private notifyAuthChange() {
    this.authSubject.next({
      isAuthenticated: this.isAuthenticated(),
      fullName: this.getFullName(),
      role: this.getRole()
    });
  }

  login(credentials: { email: string; password: string }, returnUrl?: string): Observable<AuthResponse> {
    return this.apiService.post<AuthResponse>('/auth/login', credentials).pipe(
      tap((response) => {
        this.tokenService.setToken(response.token);
        this.notifyAuthChange();
        this.toastService.success('Successfully logged in!');
        if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
        } else {
          this.redirectBasedOnRole();
        }
      }),
      catchError((error) => {
        this.toastService.error(error.error?.message || 'Login failed.');
        return throwError(() => error);
      })
    );
  }

  register(user: { email: string; password: string; name: string; role?: string }): Observable<AuthResponse> {
    const payload = {
      email: user.email,
      password: user.password,
      fullName: user.name,
      role: user.role || 'User'
    };
    return this.apiService.post<AuthResponse>('/auth/register', payload).pipe(
      tap((response) => {
        this.tokenService.setToken(response.token);
        this.notifyAuthChange();
        this.toastService.success('Registration successful!');
        this.redirectBasedOnRole();
      }),
      catchError((error) => {
        this.toastService.error(error.error?.message || 'Registration failed.');
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    this.tokenService.removeToken();
    this.notifyAuthChange();
    this.toastService.info('You have been logged out.');
    this.router.navigate(['/auth/login']);
  }

  isAuthenticated(): boolean {
    return !this.tokenService.isTokenExpired();
  }

  hasRole(role: string): boolean {
    return this.tokenService.getRole() === role;
  }

  getRole(): string | null {
    return this.tokenService.getRole();
  }

  getFullName(): string | null {
    return this.tokenService.getFullName();
  }

  private redirectBasedOnRole(): void {
    const role = this.getRole();
    if (role === 'Admin') {
      this.router.navigate(['/admin']);
    } else if (role === 'HotelManager') {
      this.router.navigate(['/manager']);
    } else {
      this.router.navigate(['/home']);
    }
  }

  forgotPassword(email: string): Observable<any> {
    return this.apiService.post('/auth/forgot-password', { email });
  }

  verifyOtp(email: string, otp: string): Observable<any> {
    return this.apiService.post('/auth/verify-otp', { email, otp });
  }

  resetPassword(email: string, otp: string, newPassword: string): Observable<any> {
    return this.apiService.post('/auth/reset-password', { email, otp, newPassword });
  }
}
