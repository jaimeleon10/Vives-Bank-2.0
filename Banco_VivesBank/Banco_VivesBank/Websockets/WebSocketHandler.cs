using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Banco_VivesBank.Websockets;

public class WebSocketHandler
{
    private readonly WebSocket _webSocket;
    private static readonly ConcurrentDictionary<WebSocket, string> _sockets = new();
    private static readonly ConcurrentDictionary<string, List<WebSocket>> _userSockets = new();
    private readonly string _username;

    public WebSocketHandler(WebSocket webSocket, string username)
    {
        _webSocket = webSocket;
        _username = username;

        _sockets[_webSocket] = _username;

        _userSockets.AddOrUpdate(username,
            new List<WebSocket> { webSocket },
            (key, existingList) =>
            {
                existingList.Add(webSocket);
                return existingList;
            });
    }

    public async Task Handle()
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (result.CloseStatus.HasValue)
                    break;

                var serverMessage = Encoding.UTF8.GetBytes($"Server: Received at {DateTime.Now}");
                await _webSocket.SendAsync(new ArraySegment<byte>(serverMessage, 0, serverMessage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
            }
        }
        finally
        {
            await CloseConnection();
        }
    }

    private async Task CloseConnection()
    {
        _sockets.TryRemove(_webSocket, out var username);

        if (!string.IsNullOrEmpty(username) && _userSockets.TryGetValue(username, out var userSockets))
        {
            userSockets.Remove(_webSocket);
            if (userSockets.Count == 0)
                _userSockets.TryRemove(username, out _);
        }

        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
    }

    public static async Task SendToCliente(string username, Notificacion notificacion)
    {
        if (_userSockets.TryGetValue(username, out var userConnections))
        {
            var jsonMessage = JsonSerializer.Serialize(notificacion);
            var messageBuffer = Encoding.UTF8.GetBytes(jsonMessage);

            foreach (var client in userConnections)
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(new ArraySegment<byte>(messageBuffer, 0, messageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}