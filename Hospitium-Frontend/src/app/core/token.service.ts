import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class TokenService {
  private readonly TOKEN_KEY = 'authToken';

  private get isBrowser(): boolean {
    return typeof window !== 'undefined' && typeof window.localStorage !== 'undefined';
  }

  setToken(token: string): void {
    if (!this.isBrowser) return;
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  getToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem(this.TOKEN_KEY);
  }

  removeToken(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem(this.TOKEN_KEY);
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;
    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp < currentTime;
    } catch {
      return true;
    }
  }

  getRole(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decoded: any = jwtDecode(token);
      // Backend uses 'Role' claim (PascalCase custom claim)
      return decoded['Role'] ?? decoded['role'] ?? decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null;
    } catch {
      return null;
    }
  }

  getUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decoded: any = jwtDecode(token);
      return decoded['UserId'] ?? decoded['sub'] ?? null;
    } catch {
      return null;
    }
  }

  getFullName(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decoded: any = jwtDecode(token);
      return decoded['FullName'] ?? decoded['name'] ?? null;
    } catch {
      return null;
    }
  }
}
