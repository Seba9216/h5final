import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

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

  username = signal<string | null>(null);
  errorMessage = signal<string>('');
  isLoading = signal<boolean>(true);

  constructor() {
    this.loadProfile();
  }

private loadProfile() {
  const token = this.authService.getToken();
  const userId = this.authService.getUserId();

  if (!token || !userId) {
    this.router.navigate(['/login']);
    return;
  }

  this.http.get<any>(`http://localhost:5057/api/user/${userId}`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
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
