import { Injectable } from '@angular/core';
import { Ducker } from '../../models/duckrace/ducker';
import { BehaviorSubject, Subject } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class GameSocketService {
  private SERVER_URL = "ws://localhost:5057/ws";
  private ws: WebSocket | null = null;

  private playersMap = new Map<string, Ducker>();
  private playersSubject = new BehaviorSubject<Ducker[]>([]);
  public players$ = this.playersSubject.asObservable();

  private lobbyCodeSubject = new BehaviorSubject<string>('');
  public lobbyCode$ = this.lobbyCodeSubject.asObservable();

  private gameStartedSubject = new Subject<Ducker[]>();
  public gameStarted$ = this.gameStartedSubject.asObservable();

  constructor(private authService: AuthService) { }

  public createGame() {
    this.setupWebSocket();
    this.sendWhenOpen({ type: "create_lobby", LobbyType: "DuckRace" });
    this.playersMap.clear();
    this.updatePlayers();
  }

  public joinGame(gamePin: string, duckerName: string) {
    this.setupWebSocket();
    this.sendWhenOpen({
      type: "join_lobby",
      LobbyCode: +gamePin,
      DuckerName: duckerName
    });
  }

  public sendStoryPoints(storyPoints: string){
    this.setupWebSocket();
    this.sendWhenOpen({type: "story_points", StoryPoints: storyPoints })
  }

  public startGame(gamePin: string) {
    this.sendWhenOpen({ type: "start_game", LobbyCode: +gamePin });
  }

  private setupWebSocket() {
    if (this.ws && (this.ws.readyState === WebSocket.OPEN || this.ws.readyState === WebSocket.CONNECTING)) {
      return; 
    }

    this.ws = new WebSocket(this.SERVER_URL);

    this.ws.addEventListener("message", (event) => this.handleMessage(event));
    this.ws.addEventListener("error", (error) => console.error("WebSocket error:", error));
    this.ws.addEventListener("close", () => console.log("WebSocket closed"));
  }

  private handleMessage(event: MessageEvent) {
    const message = JSON.parse(event.data.toString());
    console.log("Received:", message);

    switch (message.Type) {
      case "lobby_created":
        this.lobbyCodeSubject.next(message.LobbyCode);
        break;

      case "joined_lobby":
        this.playersMap.clear();
        
        // Add existing players
        message.ConnectedPlayers.forEach((ducker: any) => {
          this.playersMap.set(ducker.ConnectionId, {
            connectionId: ducker.ConnectionId,
            name: ducker.DuckerName,
            speed: ducker.Speed,
            storyPoints: null,
            userId: ducker.UserId ?? null
          });
        });
        
      
        this.updatePlayers();
        break;
case "story_points": {
  const player = this.playersMap.get(message.Player.ConnectionId);

  if (player) {
    this.playersMap.set(message.Player.ConnectionId, {
      ...player,
      storyPoints: message.StoryPoints,
    });
  }
  break;
}

      case "player_joined":
        this.playersMap.set(message.Player.ConnectionId, {
          connectionId: message.Player.ConnectionId,
          name: message.Player.DuckerName,
          speed: message.Player.Speed,
          storyPoints: null,
          userId: message.Player.UserId ?? null
        });
        this.updatePlayers();
        break;

      case "player_left":
        this.playersMap.delete(message.ConnectionId);
        this.updatePlayers();
        break;

      case "start_game":
        const duckers: Ducker[] = message.Players.map((p: any) => ({
          connectionId: p.ConnectionId,
          name: p.DuckerName,
          speed: p.Speed,
          storyPoints: null,
          userId: p.UserId ?? null
        }));
        this.gameStartedSubject.next(duckers);
        break;

      default:
        console.warn("Unknown message type:", message.Type);
    }
  }

  private updatePlayers() {
    this.playersSubject.next(Array.from(this.playersMap.values()));
  }

  private sendWhenOpen(payload: object) {
    if (!this.ws) return;

    const message = {
      ...payload,
      Token: this.authService.getToken()
    };

    if (this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(message));
    } else {
      this.ws.addEventListener("open", () => {
        this.ws?.send(JSON.stringify(message));
      }, { once: true });
    }
  }
}