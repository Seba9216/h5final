import { Component, ChangeDetectorRef, Output, EventEmitter, OnInit, OnDestroy, Input, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Ducker } from '../../../models/duckrace/ducker';
import { GameSocketService } from '../../services/game-socket-service';

@Component({
  selector: 'app-connection-area',
  imports: [CommonModule, FormsModule],
  templateUrl: './connection-area.html',
})
export class ConnectionArea implements OnInit, OnDestroy {
  // UI Bindings expected by HTML
  gamePin = '';
  duckerName = '';
  newGamePin = '';
  private _currentPlayers: Ducker[] = [];
  @Input() gameType: string | null = null;
  @Input() task! : string;
  @Output() gameStarted = new EventEmitter<Ducker[]>();
  @Output() gameEndend = new EventEmitter<void>();

  // To manage memory and observables
  private subs = new Subscription();

  constructor(
    private cdr: ChangeDetectorRef,
    private socketService: GameSocketService
  ) { }

  ngOnInit(): void {
    this.subs.add(
      this.socketService.players$.subscribe(players => {
        this._currentPlayers = players;
        this.cdr.detectChanges(); // Force UI update
      })
    );

    // 2. Subscribe to Lobby Code creation
    this.subs.add(
      this.socketService.lobbyCode$.subscribe(code => {
        this.newGamePin = code;
        this.cdr.detectChanges();
      })
    );

    // 3. Subscribe to Game Start event
    this.subs.add(
      this.socketService.gameStarted$.subscribe(duckers => {
        this.gameStarted.emit(duckers);
      })
    );
 this.subs.add(
  this.socketService.gameEndend$.subscribe(() => {
    this._currentPlayers = [];
    this.newGamePin = '';
    this.cdr.detectChanges();
  })
);
  }


  ngOnDestroy(): void {
    // Clean up listeners when component is destroyed
    this.subs.unsubscribe();
  }

  // --- Methods called by HTML buttons ---

  public CreateGame() {
    this.socketService.createGame(this.gameType);
  }

  public JoinGame() {
    if (this.gamePin && this.duckerName) {

      
      this.socketService.joinGame(this.gamePin, this.duckerName, this.gameType ?? "null");
    } else {
      console.warn("Name and Pin required");
    }
  }

  public StartGame() {
    // Use the local newGamePin that was updated via subscription
    if (this.newGamePin) {
      this.socketService.startGame(this.newGamePin,this.task);
    }
  }
 
  // --- Getter expected by HTML ---
  
  get playersArray(): Ducker[] {
    return this._currentPlayers;
  }
}