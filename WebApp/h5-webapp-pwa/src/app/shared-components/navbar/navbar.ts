import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';


@Component({
  selector: 'app-navbar',
  imports: [RouterLinkActive, RouterLink],
  templateUrl: './navbar.html',
})
export class Navbar {

  constructor(private authService: AuthService,
  ) {

  }

  IsLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }
  Logout() {
    this.authService.logout()
  }
}
