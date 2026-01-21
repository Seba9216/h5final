import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
   selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home-page.html', 

})
export class HomePage implements OnInit {
  raceStarted = false;
  ducks = [
    { id: 1, speed: 0, color: '#fbe405' },
    { id: 2, speed: 0, color: '#ff5733' },
    { id: 3, speed: 0, color: '#33ff57' },
    { id: 4, speed: 0, color: '#3357ff' },
    { id: 5, speed: 0, color: '#f033ff' }
  ];

  ngOnInit() {
    this.startRaceCycle();
  }

  startRaceCycle() {
    // 1. Assign random speeds
    this.ducks = this.ducks.map(duck => ({
      ...duck,
      speed: Math.random() * (6 - 3) + 3 // Speeds between 3s and 6s
    }));

    // 2. Start the race
    setTimeout(() => {
      this.raceStarted = true;
      
      // 3. Find the slowest duck to know when the race is "over"
      const slowestTime = Math.max(...this.ducks.map(d => d.speed));

      // 4. Wait for the race to finish + a small pause to celebrate
      setTimeout(() => {
        this.resetRace();
      }, (slowestTime * 1000) + 2000); 

    }, 100); 
  }

  resetRace() {
    this.raceStarted = false;
    // 5. Wait for the CSS reset transition (1.2s) to finish before restarting
    setTimeout(() => {
      this.startRaceCycle();
    }, 1500);
  }
}