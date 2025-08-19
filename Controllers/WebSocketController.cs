using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

[Route("ws")]
public class WebSocketController : ControllerBase
{
    private readonly RoomManager _roomManager;

    public WebSocketController(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    [HttpGet("join")]
    public async Task Get([FromQuery] string gameId)
    {
        Console.WriteLine(gameId);
        Console.WriteLine("WS request met");
        if (!_roomManager.GameExists(gameId))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Try to add the client to the room.
            var added = _roomManager.AddClient(gameId, webSocket);
            await _roomManager.Listen(gameId, webSocket, _roomManager.GetClientNum(gameId)-1);
            if (!added)
            {
                Console.WriteLine("Not added to room");
                // Room is full, gracefully close the connection.
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    "Game room is full.",
                    CancellationToken.None);
                return;
            }

            // await Echo(webSocket, gameId);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    // private async Task Echo(WebSocket webSocket, string gameId)
    // {
    //     var buffer = new byte[1024 * 4];
    //     var receiveResult = await webSocket.ReceiveAsync(
    //         new ArraySegment<byte>(buffer), CancellationToken.None);

    //     // Keep listening as long as the client wants to talk
    //     while (!receiveResult.CloseStatus.HasValue)
    //     {
    //         // --- This is where you process incoming messages ---
    //         // For now, we just echo the message back to the sender
    //         var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
    //         Console.WriteLine($"Received from client: {message}");

    //         var serverMsg = Encoding.UTF8.GetBytes($"Server got your message: {message}");
    //         await webSocket.SendAsync(
    //             new ArraySegment<byte>(serverMsg, 0, serverMsg.Length),
    //             receiveResult.MessageType,
    //             receiveResult.EndOfMessage,
    //             CancellationToken.None);
    //         // --- End of message processing ---

    //         // Continue listening for the next message
    //         receiveResult = await webSocket.ReceiveAsync(
    //             new ArraySegment<byte>(buffer), CancellationToken.None);
    //     }

    //     // The client wants to close the connection, so we acknowledge it.
    //     await webSocket.CloseAsync(
    //         receiveResult.CloseStatus.Value,
    //         receiveResult.CloseStatusDescription,
    //         CancellationToken.None);

    //     _roomManager.RemoveClient(gameId, webSocket);
        
    //     // TODO: Here you need to remove the webSocket from your RoomManager
    // }
}