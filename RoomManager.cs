
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
                return false;
            }
            room.Add(socket);
            return true;
        }
        return false;
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


}