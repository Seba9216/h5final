import { Component } from '@angular/core';
import { ConnectionArea } from '../../shared-components/connection-area/connection-area';

@Component({
  selector: 'app-duck-race-page',
  imports: [ConnectionArea],
  templateUrl: './duck-race-page.html',
})
export class DuckRacePage {
      gameHasStarted = false;
    onGameStarted(started: boolean) {
    this.gameHasStarted = started;
    console.log('Game started:', this.gameHasStarted);
  }
}
