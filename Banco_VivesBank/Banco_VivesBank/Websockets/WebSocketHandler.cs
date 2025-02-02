using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Banco_VivesBank.Websockets;

/// <summary>
/// Maneja las conexiones WebSocket, gestionando los clientes conectados y enviando mensajes a través de WebSocket.
/// </summary>
public class WebSocketHandler
{
    private readonly WebSocket _webSocket;
    /// <summary>
    /// Diccionario concurrente que mantiene un mapeo entre los WebSockets y los nombres de usuario asociados.
    /// </summary>
    private static readonly ConcurrentDictionary<WebSocket, string> _sockets = new();
    /// <summary>
    /// Diccionario concurrente que mantiene una lista de WebSockets asociados a cada nombre de usuario.
    /// </summary>
    private static readonly ConcurrentDictionary<string, List<WebSocket>> _userSockets = new();
    private readonly string _username;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="WebSocketHandler"/> para manejar la conexión WebSocket de un usuario.
    /// </summary>
    /// <param name="webSocket">El WebSocket asociado con la conexión.</param>
    /// <param name="username">El nombre de usuario asociado con la conexión WebSocket.</param>
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

    /// <summary>
    /// Maneja la recepción de mensajes del cliente y envía respuestas al cliente a través de WebSocket.
    /// </summary>
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

    /// <summary>
    /// Cierra la conexión WebSocket de manera ordenada, eliminando la entrada correspondiente del diccionario y cerrando la conexión.
    /// </summary>
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

    /// <summary>
    /// Envía una notificación a todos los WebSockets asociados con un usuario específico.
    /// </summary>
    /// <param name="username">El nombre de usuario al que se enviará la notificación.</param>
    /// <param name="notificacion">La notificación a enviar.</param>
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