import { ChangeDetectorRef, Component, ViewChild } from '@angular/core';
import { ConnectionArea } from '../../shared-components/connection-area/connection-area';
import { Ducker } from '../../../models/duckrace/ducker';
import { RaceFinishDirective } from '../../directives/race-finish-directive';
import { WinnerModal } from '../../shared-components/winner-modal/winner-modal';
import { GameSocketService } from '../../services/game-socket-service';

@Component({
  selector: 'app-duck-race-page',
  imports: [ConnectionArea,RaceFinishDirective,WinnerModal],
  templateUrl: './duck-race-page.html',
})
export class DuckRacePage {
     @ViewChild('winnerModal') winnerModal!: WinnerModal;
      gameHasStarted: boolean = false;
      gameHasFinished = false;
      players: Ducker[] = [];

    constructor(private cdr: ChangeDetectorRef, private gameSocketService: GameSocketService) { }
    onGameStartedLoadPlayers(started: Ducker[]) {
    this.players = started;
    console.log('Game started:', this.players);
    this.gameHasStarted = true;
    this.cdr.markForCheck();
  }
   onDuckFinished(finishedDuck: Ducker){
    if(!this.gameHasFinished){
      this.gameHasFinished = true;
    this.gameSocketService.gameFinished();
    this.winnerModal.duckerName = finishedDuck.name;
    this.winnerModal.show();
    }
  }
}
