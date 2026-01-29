import { Component, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-connection-area',
  imports: [CommonModule],
  templateUrl: './connection-area.html',
})
export class ConnectionArea {
  private SERVER_URL = "ws://localhost:5057/ws";
  gamePin = '';
  duckerName = '';
  newGamePin = '';
  players: string[] = [];
  private ws: WebSocket | null = null;
  @Output() gameStarted = new EventEmitter<boolean>();

  
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
        this.players = [this.duckerName, ...message.ConnectedPlayers];
        this.cdr.detectChanges();
        break;

      case "player_joined":
        this.players.push(message.PlayerName);
        this.cdr.detectChanges();
        break;
      case "player_left":
        this.players = this.players.filter(p => p !== message.PlayerName);
        this.cdr.detectChanges();
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
    this.sendWhenOpen({ type: "start_game" });
    this.gameStarted.emit(true);
  
  }

  public CreateGame() {
    this.setupWebSocket();
    this.sendWhenOpen({ type: "create_lobby" });
    this.players = [];
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
}