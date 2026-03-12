import { Injectable } from '@angular/core';
import { Ducker } from '../../models/duckrace/ducker';
import { BehaviorSubject, Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment'

@Injectable({
  providedIn: 'root'
})
export class GameSocketService {
  private SERVER_URL = environment.WebSocketUrl;
  private ws: WebSocket | null = null;

  private playersMap = new Map<string, Ducker>();
  private playersSubject = new BehaviorSubject<Ducker[]>([]);
  public players$ = this.playersSubject.asObservable();

  private lobbyCodeSubject = new BehaviorSubject<string>('');
  public lobbyCode$ = this.lobbyCodeSubject.asObservable();

  private gameStartedSubject = new Subject<Ducker[]>();
  public isHost = false;
  public task!: string; 
  public gameStarted$ = this.gameStartedSubject.asObservable();

  private gameEndedSubject = new Subject<void>();
  public gameEndend$ = this.gameEndedSubject.asObservable();

  private newRoundSubject = new Subject<void>();
  public newRound$ = this.newRoundSubject.asObservable();

  private revealCardsSubject = new Subject<void>();
  public revealCards$ = this.revealCardsSubject.asObservable();

  constructor(private authService: AuthService) { }

  public createGame(gameType : string |null) {
    this.setupWebSocket();
    
    this.sendWhenOpen({ type: "create_lobby", LobbyType: gameType });
    this.playersMap.clear();
    this.updatePlayers();
  }

  public joinGame(gamePin: string, duckerName: string, lobbyType: string) {
    this.setupWebSocket();
    this.sendWhenOpen({
      type: "join_lobby",
      LobbyCode: +gamePin,
      DuckerName: duckerName,
      LobbyType: lobbyType
    });
  }



  public sendStoryPoints(storyPoints: string){
    this.sendWhenOpen({type: "story_points", StoryPoints: storyPoints })
    this.updatePlayers();
  }

  public startGame(gamePin: string, task : string) {
    this.sendWhenOpen({ type: "start_game", LobbyCode: + gamePin, Task: task });
  }
  public revealCards(){
    this.sendWhenOpen({type: "cards_reveal"});
  }
  public gameFinished()
  {
    this.sendWhenOpen({ type: "game_finished" });
  }
  public newRound(){
    this.sendWhenOpen({ type: "new_round"});
  }
  public closeWebSocketConnection(){
    this.playersMap.clear();
    this.isHost = false;
    
    this.ws?.close();
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
        this.isHost = true;
        console.log(this.isHost);
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
  const player = this.playersMap.get(message.ConnectionId);

  if (player) {
    this.playersMap.set(message.ConnectionId, {
      ...player,
      storyPoints: message.StoryPoints,
    });
  }
  this.updatePlayers();
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
      case "reveal_cards":
        this.revealCardsSubject.next();
        break;
      case "player_left":
        this.playersMap.delete(message.ConnectionId);
        console.log('players left ' + message.ConnectionId)
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
        this.task = message.Task;
        this.gameStartedSubject.next(duckers);
        break;
case "finished_game":
  this.playersMap.clear();        // ← add this
  this.updatePlayers();           // ← and this, so players$ emits []
  this.gameEndedSubject.next();
  break;
      break;
      case "start_new_round":
          this.newRoundSubject.next();
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
