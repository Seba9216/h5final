import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private isAuthenticated = false;

  constructor(private router: Router) {
    // Check localStorage for existing session
    const token = localStorage.getItem('authToken');
    this.isAuthenticated = !!token;
  }

  login(userId: number, userName: string): void {
    this.isAuthenticated = true;
    localStorage.setItem('authToken', 'true');
    localStorage.setItem('userId', userId.toString());
    localStorage.setItem('userName', userName);
  }

  logout(): void {
    this.isAuthenticated = false;
    localStorage.removeItem('authToken');
    localStorage.removeItem('userId');
    localStorage.removeItem('userName');
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated;
  }

  getUserId(): number | null {
    const userId = localStorage.getItem('userId');
    return userId ? parseInt(userId, 10) : null;
  }

  getUserName(): string | null {
    return localStorage.getItem('userName');
  }
}
