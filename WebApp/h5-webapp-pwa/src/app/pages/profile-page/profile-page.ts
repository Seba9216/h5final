import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { DuckingLogin } from '../../../models/ducking-login.model';
import { DuckingGameHistoryModel } from '../../../models/ducking-game-history.model';
import { environment } from '../../../environments/environment';  // ← ADD THIS

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-page.html',
})
export class ProfilePage {
  private http = inject(HttpClient);
  private router = inject(Router);
  private authService = inject(AuthService);
  gameHistory = signal<DuckingGameHistoryModel | null>(null);
  userLogins = signal<DuckingLogin[]>([]);
  username = signal<string | null>(null);
  errorMessage = signal<string>('');
  isLoading = signal<boolean>(true);

  constructor() {
    this.loadProfile();
    this.loadGameHistory();
    this.loadUserLogins();
  }

  private loadUserLogins() {
    const token = this.authService.getToken();
    const userId = this.authService.getUserId();
    this.http.get<any[]>(`${environment.ApiUrl}/LoginHistory/${userId}`, { 
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: (data) => this.userLogins.set(data),
      error: () => console.error('Failed to load logins')
    });
  }

  private loadGameHistory() {
    const token = this.authService.getToken();
    const userId = this.authService.getUserId();
    this.http.get<any>(`${environment.ApiUrl}/gamehistory/${userId}`, {  // ← CHANGED
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: (data) => this.gameHistory.set(data),
      error: () => console.error('Failed to load game history')
    });
  }

  private loadProfile() {
    const token = this.authService.getToken();
    const userId = this.authService.getUserId();
    if (!token || !userId) {
      this.router.navigate(['/login']);
      return;
    }
    this.http.get<any>(`${environment.ApiUrl}/user/${userId}`, {  // ← CHANGED
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: (data) => {
        this.username.set(data.userName);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load profile');
        this.isLoading.set(false);
      }
    });
  }
}