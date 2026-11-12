using System.Net.WebSockets;
using System.Text;

public class ChatService
{
    private readonly List<WebSocket> _clients = new List<WebSocket>();
    private readonly object _clientsLock = new object();
    private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
    private readonly ILogger<ChatService> _logger;

    public ChatService(ILogger<ChatService> logger)
    {
        _logger = logger;
    }

    public async Task HandleClient(WebSocket socket, CancellationToken cancellationToken)
    {
        lock (_clientsLock)
        {
            _clients.Add(socket);
        }

        byte[] buffer = new byte[4096];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", cancellationToken);
                    break;
                }

                if (result.EndOfMessage == false)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message is too big", cancellationToken);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await SendToAll(message);
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogDebug(ex, "aaaaaaaaaaaaaaa");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "aaaaaaaaaaaaaaa");
        }
        finally
        {
            lock (_clientsLock)
            {
                _clients.Remove(socket);
            }
        }
    }

    private async Task SendToAll(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        List<WebSocket> clients;
        lock (_clientsLock)
        {
            clients = _clients.ToList();
        }

        await _sendLock.WaitAsync();
        try
        {
            foreach (WebSocket client in clients)
            {
                if (client.State != WebSocketState.Open)
                {
                    continue;
                }

                try
                {
                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (WebSocketException ex)
                {
                    _logger.LogDebug(ex, "bbbbbbbbbb");
                }
            }
        }
        finally
        {
            _sendLock.Release();
        }
    }
}
