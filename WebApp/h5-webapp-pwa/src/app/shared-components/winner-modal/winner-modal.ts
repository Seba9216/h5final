import { Component, inject, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-winner-modal',
  imports: [],
  templateUrl: './winner-modal.html',
})
export class WinnerModal {
  @Input() duckerName!: string | null;
  @Input() closeureText! :string;
  isVisible = false;
  private router = inject(Router);
  show() {
    this.isVisible = true;
  }
  
  close() {
   this.router.navigate(['/']);
    this.isVisible = false;
  }
}