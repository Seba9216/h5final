import { Component } from '@angular/core';
import { ConnectionArea } from '../../shared-components/connection-area/connection-area';
import { Ducker } from '../../../models/duckrace/ducker';

@Component({
  selector: 'app-duck-race-page',
  imports: [ConnectionArea],
  templateUrl: './duck-race-page.html',
})
export class DuckRacePage {
      gameHasStarted: boolean = false;
      players: Ducker[] = [];
    onGameStartedLoadPlayers(started: Ducker[]) {
    this.players = started;
    console.log('Game started:', this.players);
    this.gameHasStarted = true;
  }
}
