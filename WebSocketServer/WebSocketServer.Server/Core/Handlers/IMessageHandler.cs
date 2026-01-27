namespace WebSocketServer.Core.Handlers;

public interface IMessageHandler
{
    Task HandleMessageAsync(string connectionId, string messageType, string messageJson);
}