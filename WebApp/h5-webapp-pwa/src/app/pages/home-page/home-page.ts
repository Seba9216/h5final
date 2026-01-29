import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomePage implements OnInit {
  raceStarted = false;

  ducks = [
    { connectionId: "1", speed: 0, color: '#fbe405' },
    { connectionId: "2", speed: 0, color: '#ff5733' },
    { connectionId: "3", speed: 0, color: '#33ff57' },
    { connectionId: "4", speed: 0, color: '#3357ff' },
    { connectionId: "5", speed: 0, color: '#f033ff' }
  ];

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.startRaceCycle();
  }

  startRaceCycle() {
    this.ducks = this.ducks.map(duck => ({
      ...duck,
      speed: Math.random() * (6 - 3) + 3
    }));
    this.cdr.markForCheck();

    setTimeout(() => {
      this.raceStarted = true;
      this.cdr.markForCheck();

      const slowestTime = Math.max(...this.ducks.map(d => d.speed));

      setTimeout(() => {
        this.resetRace();
      }, slowestTime * 1000 + 2000);
    }, 100);
  }

  resetRace() {
    this.raceStarted = false;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.startRaceCycle();
    }, 1500);
  }
}
