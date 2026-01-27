import { Component } from '@angular/core';
import { linkedSignalUpdateFn } from '@angular/core/primitives/signals';
import { RouterLink, RouterLinkActive } from '@angular/router';
import {
	NgbNavContent,
	NgbNav,
	NgbNavItem,
	NgbNavItemRole,
	NgbNavLinkButton,
	NgbNavLinkBase,
	NgbNavOutlet,
} from '@ng-bootstrap/ng-bootstrap/nav';


@Component({
  selector: 'app-navbar',
  	imports: [NgbNavContent, NgbNav, NgbNavItem, NgbNavItemRole, NgbNavLinkButton, NgbNavLinkBase, NgbNavOutlet,RouterLinkActive,RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {

}
