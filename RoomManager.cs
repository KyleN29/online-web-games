
using System.Collections.Concurrent;
using System.Net.WebSockets;

public class RoomManager
{
    private ConcurrentDictionary<string, ConcurrentBag<WebSocket>> _clientsByGame = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="socket"></param>
    /// <returns>True if the client was successfully added, false otherwise</returns>
    public bool AddClient(string gameId, WebSocket socket)
    {
        if (_clientsByGame.TryGetValue(gameId, out var room))
        {
            // Only allow 2 players in a game
            if (room.Count >= 2)
            {
                Console.WriteLine("TOO MANY!!!");
                return false;
            }

            room.Add(socket);

            // If room now has 2 clients, notify both
            if (room.Count == 2)
            {
                Console.WriteLine("Perfect 😎");
                _ = NotifyRoomReady(room);
            }

            return true;
        }
        return false;
    }

    public void RemoveClient(string gameId, WebSocket socket)
    {
        if (_clientsByGame.TryGetValue(gameId, out var room))
        {
            var newRoom = new ConcurrentBag<WebSocket>();
            foreach (var s in room)
            {
                if (s != socket)
                    newRoom.Add(s);
            }

            _clientsByGame[gameId] = newRoom;
            Console.WriteLine("Removed from room");
        }
    }


    public int GetClientNum(string gameId)
    {
        if (_clientsByGame.TryGetValue(gameId, out var room))
        {
            return room.Count;
        }
        return -1;
    }

    public string GenerateGame()
    {
        var gameId = Guid.NewGuid().ToString()[..8];
        _clientsByGame[gameId] = new ConcurrentBag<WebSocket>();
        return gameId;
    }

    public bool GameExists(string gameId)
    {
        return _clientsByGame.TryGetValue(gameId, out var _);
    }

    private async Task NotifyRoomReady(ConcurrentBag<WebSocket> room)
    {
        var message = new { type = "room_ready", players = room.Count };

        foreach (var socket in room)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendJsonAsync(message);
            }
        }
    }


}