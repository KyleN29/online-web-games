using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public static class WebSocketExtensions
{
    public static async Task SendJsonAsync<T>(
        this WebSocket socket,
        T payload,
        CancellationToken cancellationToken = default)
    {
        if (socket.State != WebSocketState.Open)
            return;

        string json = JsonSerializer.Serialize(payload);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await socket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true, // end of message
            cancellationToken
        );
    }

    public static async Task<T?> ReceiveJsonAsync<T>(
        this WebSocket socket,
        byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        var result = await socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), cancellationToken);

        if (result.MessageType == WebSocketMessageType.Close)
            return default;

        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
        return JsonSerializer.Deserialize<T>(json);
    }
}
