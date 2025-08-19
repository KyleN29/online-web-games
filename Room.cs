using System.Drawing;
using System.Net.WebSockets;
using System.Threading.Tasks;

public class Room
{
    public string GameId;
    private List<WebSocket> _clients = new();
    private List<SnakeGame> _games = new();

    public Room(string gameId, int gridSize)
    {
        GameId = gameId;
    }

    private async Task NotifyRoomReady()
    {
        var message = new { type = "room_ready" };

        foreach (var socket in _clients)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendJsonAsync(message);
            }
        }
    }

    public async Task AddClient(WebSocket socket)
    {
        if (_clients.Count >= 2) throw new InvalidOperationException("Room full");

        _clients.Add(socket);
        _games.Add(new SnakeGame(12, _clients.Count-1, this));


        // Let all clients know how many players are currently connected
        var message = new { type = "player_count", playerCount = _clients.Count };
        for (int i = 0; i < _clients.Count; i++)
        {
            await _clients[i].SendJsonAsync(message);
        }
        
        

        if (_clients.Count == 2)
        {
            await NotifyRoomReady();
            _ = StartGameLoop();
        }
    }

    public void RemoveClient(WebSocket socket)
    {
        List<WebSocket> newClients = new List<WebSocket>();
        foreach (var s in _clients)
        {
            if (s != socket)
            {
                newClients.Add(s);
            }
        }
        _clients = newClients;
    }

    public int ClientCount()
    {
        return _clients.Count;
    }

    public async Task StartGameLoop()
    {
        Console.WriteLine("Game loop starting...");
        // Initialize loop to listen for inputs
        for (int i = 0; i < 2; i++)
        {
            _ = Task.Run(() => ReceiveDirection(_clients[i], i));
        }
        while (_clients.Count == 2) // run as long as 2 clients connected
            {
            Console.WriteLine("WERE ROLLING");
                for (int i = 0; i < 2; i++)
            {
                _games[i].Step();
                var state = new
                {
                    type = "game_data",
                    grid = _games[i].grid,
                    snake = _games[i].snakeBody,
                    gameOver = _games[i].gameOver,
                    direction = _games[i].currentDirection
                };

                var msg = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(state);
                if (_clients[i].State == WebSocketState.Open)
                {
                    await _clients[i].SendAsync(
                        new ArraySegment<byte>(msg),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }

                await Task.Delay(170); // ~6 ticks per second
            }

        Console.WriteLine("Game loop ended (not enough clients).");
    }

    public void IncreaseOpponentSnakeLength(int gameNum)
    {
        _games[gameNum].IncreaseLength();
    }

    private class DirectionMessage
    {
        public SnakeGame.Point direction { get; set; }
    }

    public async Task ReceiveDirection(WebSocket webSocket, int gameNum)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveJsonAsync<DirectionMessage>(buffer);

        // Keep listening as long as the client wants to talk
        while (!webSocket.CloseStatus.HasValue)
        {

            if (receiveResult?.direction != null)
            {
                var direction = receiveResult.direction;

                var curGame = _games[gameNum];
                Console.WriteLine("Last Direction: " + curGame.lastDirection.X + "," + curGame.lastDirection.Y + " - Cur Direction: " + curGame.currentDirection.X + "," + curGame.currentDirection.Y);

                curGame.SetNewDirection(direction);
                if (direction.X != curGame.lastDirection.X && direction.Y != curGame.lastDirection.Y)
                {
                    curGame.currentDirection = direction;
                }

            }
            
            receiveResult = await webSocket.ReceiveJsonAsync<DirectionMessage>(buffer);
        }
        // TODO: Here you need to remove the webSocket from your RoomManager
    }


}