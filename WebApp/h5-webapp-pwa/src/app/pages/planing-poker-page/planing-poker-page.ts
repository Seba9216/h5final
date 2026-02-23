import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { Ducker } from '../../../models/duckrace/ducker';
import { ConnectionArea } from '../../shared-components/connection-area/connection-area';
import { GameSocketService } from '../../services/game-socket-service';
import { throwError } from 'rxjs';
import { WinnerModal } from '../../shared-components/winner-modal/winner-modal';

@Component({
  selector: 'app-planing-poker-page',
  standalone: true,
  imports: [ConnectionArea,WinnerModal],
  templateUrl: './planing-poker-page.html',
})
export class PlaningPokerPage implements OnInit {
  @ViewChild('winnerModal') winnerModal!: WinnerModal;
  gameHasStarted = false;
  gameHasEnded = false;
  players: Ducker[] = [];
  ishost = false;
  revealed = false;
  myVote: string | null = null;

  readonly pokerValues = ['0', '½', '1', '2', '3', '5', '8', '13', '21', '?'];
  readonly duckColors = [
    '#FBE405', '#4FC3F7', '#66BB6A', '#F48FB1',
    '#CE93D8', '#EF5350', '#FFB74D', '#80CBC4'
  ];

  constructor(
    private cdr: ChangeDetectorRef,
    private socketService: GameSocketService
  ) {}
ngOnInit() {
  this.socketService.players$.subscribe(updatedPlayers => {
    if (!this.gameHasStarted) return;

    this.players = this.players.map(p => {
      const updated = updatedPlayers.find(u => u.connectionId === p.connectionId);
      return updated ? { ...p, storyPoints: updated.storyPoints } : p;
    });
    this.cdr.markForCheck();
  });
    this.socketService.revealCards$.subscribe(() => {
    this.revealed = true;
    this.cdr.markForCheck();
  });
  this.socketService.gameEndend$.subscribe(() => {
    this.gameHasEnded = true;
    this.winnerModal.closeureText = "We have an agreement!"
    this.winnerModal.duckerName = "The task is estimated at " + this.players[0].storyPoints;
    this.winnerModal.show();
    this.socketService.closeWebSocketConnection();
    this.cdr.markForCheck();
  });
   this.socketService.newRound$.subscribe(() => {
    this.revealed = false;
    this.myVote = null;

    this.players = this.players.map(p => ({
      ...p,
      storyPoints: null
    }));    
    this.cdr.markForCheck();
  });
  
}

  onGameStartedLoadPlayers(started: Ducker[]) {
    this.players = started.map(p => ({
      ...p,
      storyPoints: null
    }));

    this.gameHasStarted = true;
    this.ishost = this.socketService.isHost;
  console.log(this.ishost); 
    this.revealed = false;
    this.myVote = null;
    this.cdr.markForCheck();
  }

  getDuckColor(index: number): string {
    return this.duckColors[index % this.duckColors.length];
  }

  selectVote(value: string) {
    this.myVote = value;
    this.socketService.sendStoryPoints(value);
    console.log(this.players);
  }

  hasVoted(player: Ducker): boolean {
    return !!player.storyPoints;
  }

  getVote(player: Ducker): string {
    return player.storyPoints ?? '?';
  }

  revealCards() {
   this.socketService.revealCards();  
   const allSameStoryPoints =
  this.players.length > 0 &&
  this.players.every(p => p.storyPoints === this.players[0].storyPoints);
  if(allSameStoryPoints){
    this.socketService.gameFinished();
    this.winnerModal.show();
  } 
  }
  newRound() {
    this.socketService.newRound();
  }
  get hasAnyVotes(): boolean {
  return this.players.some(p => !!p.storyPoints);
}
}
