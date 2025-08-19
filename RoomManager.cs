
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

public class RoomManager
{
    private ConcurrentDictionary<string, Room> _gameRooms = new(); // <gameId, Room>

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="socket"></param>
    /// <returns>True if the client was successfully added, false otherwise</returns>
    public bool AddClient(string gameId, WebSocket socket)
    {
        if (_gameRooms.TryGetValue(gameId, out var room))
        {
            room.AddClient(socket);

            return true;
        }
        return false;
    }

    public void RemoveClient(string gameId, WebSocket socket)
    {
        if (_gameRooms.TryGetValue(gameId, out var room))
        {
            room.RemoveClient(socket);
        }
    }


    public int GetClientNum(string gameId)
    {
        if (_gameRooms.TryGetValue(gameId, out var room))
        {
            return room.ClientCount();
        }
        return -1;
    }

    public string GenerateGame()
    {
        var gameId = Guid.NewGuid().ToString()[..8];
        _gameRooms[gameId] = new Room(gameId, 12);
        return gameId;
    }

    public bool GameExists(string gameId)
    {
        return _gameRooms.TryGetValue(gameId, out var _);
    }

    public async Task Listen(string gameId, WebSocket socket, int gameNum)
    {
        if (_gameRooms.TryGetValue(gameId, out var room))
        {
            await room.ReceiveDirection(socket, gameNum);

        }
    }




}