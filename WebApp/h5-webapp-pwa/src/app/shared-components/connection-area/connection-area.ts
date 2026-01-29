import { Component, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Ducker } from '../../../models/duckrace/ducker';

@Component({
  selector: 'app-connection-area',
  imports: [CommonModule, FormsModule],
  templateUrl: './connection-area.html',
})
export class ConnectionArea {
  private SERVER_URL = "ws://localhost:5057/ws";
  gamePin = '';
  duckerName = '';
  newGamePin = '';
  players: Map<string, Ducker> = new Map(); // ConnectionId -> Ducker
  private ws: WebSocket | null = null;
  @Output() gameStarted = new EventEmitter<Ducker[]>();
  
  
  constructor(
    private cdr: ChangeDetectorRef
  ) { }

  private setupWebSocket() {
    if (this.ws) {
      this.ws.close();
    }

    this.ws = new WebSocket(this.SERVER_URL);

    this.ws.addEventListener("message", (event) => {
      this.handleMessage(event);
    });

    this.ws.addEventListener("error", (error) => {
      console.error("WebSocket error:", error);
    });

    this.ws.addEventListener("close", () => {
      console.log("WebSocket closed");
    });
  }

  private handleMessage(event: MessageEvent) {
    console.log(event.data);
    const message = JSON.parse(event.data.toString());
    console.log("Received:", message);

    switch (message.Type) {
      case "lobby_created":
        this.newGamePin = message.LobbyCode;
        console.log("Lobby created:", message.LobbyCode);
        this.cdr.detectChanges();
        break;

      case "joined_lobby":
        this.players.clear();
        const currentPlayer: Ducker = {
          connectionId: 'self',
          name: this.duckerName,
          speed: 0
        };
        this.players.set('self', currentPlayer);
        
        message.ConnectedPlayers.forEach((ducker: any) => {
          const player: Ducker = {
            connectionId: ducker.ConnectionId,
            name: ducker.DuckerName,
            speed: ducker.Speed
          };
          this.players.set(ducker.ConnectionId, player);
        });
        this.cdr.detectChanges();
        break;

      case "player_joined":
        const newPlayer: Ducker = {
          connectionId: message.Player.ConnectionId,
          name: message.Player.DuckerName,
          speed: message.Player.Speed
        };
        this.players.set(message.Player.ConnectionId, newPlayer);
        this.cdr.detectChanges();
        break;

      case "player_left":
        this.players.delete(message.ConnectionId);
        this.cdr.detectChanges();
        break;

      case "start_game":
        const duckers: Ducker[] = message.Players.map((p: any) => ({
          connectionId: p.ConnectionId,
          name: p.DuckerName,
          speed: p.Speed
        }));
        this.gameStarted.emit(duckers);
        break;

      default:
        console.warn("Unknown message type:", message.Type);
    }
  }

  private sendWhenOpen(payload: object) {
    if (!this.ws) return;

    if (this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(payload));
    } else {
      this.ws.addEventListener("open", () => {
        this.ws?.send(JSON.stringify(payload));
      }, { once: true });
    }
  }

  public StartGame() {
    this.sendWhenOpen({ type: "start_game", LobbyCode: +this.newGamePin });
  }

  public CreateGame() {
    this.setupWebSocket();
    this.sendWhenOpen({ type: "create_lobby" });
    this.players.clear();
    this.cdr.detectChanges();
  }

  public JoinGame() {
    this.setupWebSocket();
    this.sendWhenOpen({
      type: "join_lobby",
      LobbyCode: +this.gamePin,
      DuckerName: this.duckerName
    });
  }

  get playersArray(): Ducker[] {
    return Array.from(this.players.values());
  }
}
