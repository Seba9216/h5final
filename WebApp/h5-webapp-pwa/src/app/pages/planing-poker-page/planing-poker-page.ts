import { ChangeDetectorRef, Component } from '@angular/core';
import { ConnectionAreaPoker } from '../../shared-components/connection-area-poker/connection-area-poker';
import { Ducker } from '../../../models/duckrace/ducker';

@Component({
  selector: 'app-planing-poker-page',
  imports: [ConnectionAreaPoker],
  templateUrl: './planing-poker-page.html',
})
export class PlaningPokerPage {
  gameHasStarted = false;
  players: Ducker[] = [];
  revealed = false;
  votes = new Map<string, string>();
  myVote: string | null = null;

  readonly pokerValues = ['0', '½', '1', '2', '3', '5', '8', '13', '21', '?'];
  readonly duckColors = [
    '#FBE405', '#4FC3F7', '#66BB6A', '#F48FB1',
    '#CE93D8', '#EF5350', '#FFB74D', '#80CBC4'
  ];

  constructor(private cdr: ChangeDetectorRef) {}

  onGameStartedLoadPlayers(started: Ducker[]) {
    this.players = started;
    this.gameHasStarted = true;
    this.revealed = false;
    this.votes.clear();
    this.myVote = null;
    this.cdr.markForCheck();
  }

  getDuckColor(index: number): string {
    return this.duckColors[index % this.duckColors.length];
  }

  selectVote(value: string) {
    this.myVote = value;
    if (this.players.length > 0 && this.players[0].connectionId) {
      this.votes.set(this.players[0].connectionId, value);
    }
    // Simulate other players voting for demo
    for (let i = 1; i < this.players.length; i++) {
      const id = this.players[i].connectionId;
      if (id && !this.votes.has(id)) {
        const idx = Math.floor(Math.random() * (this.pokerValues.length - 1));
        this.votes.set(id, this.pokerValues[idx]);
      }
    }
  }

  hasVoted(player: Ducker): boolean {
    return this.votes.has(player.connectionId ?? '');
  }

  getVote(player: Ducker): string {
    return this.votes.get(player.connectionId ?? '') ?? '?';
  }

  revealCards() {
    this.revealed = true;
  }

  newRound() {
    this.revealed = false;
    this.votes.clear();
    this.myVote = null;
  }
}
