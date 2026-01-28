import { Component, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-connection-area',
  imports: [],
  templateUrl: './connection-area.html',
  styleUrl: './connection-area.scss',
})
export class ConnectionArea {
  private SERVER_URL = "ws://localhost:5057/ws";
  gamePin = '';
  duckerName = '';
  newGamePin = '';

    constructor(
    private cdr: ChangeDetectorRef
  ) {}

  public CreateGame() {
    const ws = new WebSocket(this.SERVER_URL);
    let lobbyCode: string = "";
    ws.addEventListener("open", () => {
      ws.send(JSON.stringify({
        type: "create_lobby"
      }));
    });
    ws.addEventListener("message", (data) => {
      console.log(data.data);
      const message = JSON.parse(data.data.toString());
      if (message.Type === "lobby_created") {
        lobbyCode = message.LobbyCode;

        console.log("Lobby created:", lobbyCode);
        this.newGamePin = lobbyCode;
        this.cdr.detectChanges();
      }
    });

  }

  public JoinGame() {
    const ws = new WebSocket(this.SERVER_URL);
    const lobbyCode: number = +this.gamePin;
    ws.addEventListener("open", () => {

      ws.send(JSON.stringify({
        type: "join_lobby",
        LobbyCode: lobbyCode,
        DuckerName: this.duckerName
      }));
    });
    ws.addEventListener("message", (data) => {
      console.log(data.data);
      const message = JSON.parse(data.data.toString());
      console.log("Received:", message);
      
    });
  }
}
