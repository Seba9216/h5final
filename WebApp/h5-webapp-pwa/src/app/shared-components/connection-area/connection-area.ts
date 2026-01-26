import { Component } from '@angular/core';

@Component({
  selector: 'app-connection-area',
  imports: [],
  templateUrl: './connection-area.html',
  styleUrl: './connection-area.scss',
})
export class ConnectionArea {
 private SERVER_URL = "ws://localhost:5057/ws";
   gamePin = '';



 public CreateGame(){
  const ws = new WebSocket(this.SERVER_URL);
  let lobbyCode = null;
ws.addEventListener("open", () => {
    // Ask server to create a lobby
    ws.send(JSON.stringify({
        type: "create_lobby"
    }));
});
ws.addEventListener("message", (data) => {
    console.log(data.data);
    const message = JSON.parse(data.data.toString());
    // Server responded with lobby code
    if (message.type === "lobby_created") {
        lobbyCode = message.lobby_code;

        console.log("Lobby created:", lobbyCode);
    }
});

 }

 public JoinGame(){  const ws = new WebSocket(this.SERVER_URL);
  const lobbyCode: number = +this.gamePin;
ws.addEventListener("open", () => {
    // Ask server to create a lobby
   ws.send(JSON.stringify({
    type: "join_lobby",
    lobby_code: lobbyCode
}));
});
ws.addEventListener("message", (data) => {
    console.log(data.data);
    const message = JSON.parse(data.data.toString());
    console.log("Received:", message);

});

 }

}
