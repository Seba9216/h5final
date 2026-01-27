using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;

namespace WebSocketServer.Core.Handlers;

public interface IWebSocketHandler
{
    Task HandleAsync(HttpContext context, WebSocket webSocket);
}