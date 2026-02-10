import { ChangeDetectorRef, Component } from '@angular/core';
import { Ducker } from '../../../models/duckrace/ducker';
import { ConnectionArea } from '../../shared-components/connection-area/connection-area';
import { GameSocketService } from '../../services/game-socket-service';

@Component({
  selector: 'app-planing-poker-page',
  imports: [ConnectionArea],
  templateUrl: './planing-poker-page.html',
})
export class PlaningPokerPage {
  gameHasStarted = false;
  players: Ducker[] = [];
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

  onGameStartedLoadPlayers(started: Ducker[]) {
    this.players = started.map(p => ({
      ...p,
      storyPoints: null
    }));

    this.gameHasStarted = true;
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
  }

  hasVoted(player: Ducker): boolean {
    return !!player.storyPoints;
  }

  getVote(player: Ducker): string {
    return player.storyPoints ?? '?';
  }

  revealCards() {
    this.revealed = true;
  }

  newRound() {
    this.revealed = false;
    this.myVote = null;

    this.players = this.players.map(p => ({
      ...p,
      storyPoints: null
    }));
  }
  get hasAnyVotes(): boolean {
  return this.players.some(p => !!p.storyPoints);
}
}
