using System.Net.WebSockets;

public class Room
{
    public string GameId;
    private List<WebSocket> _clients = new();
    private List<SnakeGame> _games = new();

    public Room(string gameId, int gridSize)
    {
        GameId = gameId;
    }

    public void AddClient(WebSocket socket)
    {
        if (_clients.Count >= 2) throw new InvalidOperationException("Room full");

        _clients.Add(socket);
        _games.Add(new SnakeGame(12, socket));

        if (_clients.Count == 2)
        {
            _ = StartGameLoop();
        }
    }

    public async Task StartGameLoop()
    {
        for (int i = 0; i < 2; i++)
        {
            _games[i].Step();
            var state = new
            {
                type = "game_data",
                grid = _games[i].grid,
                snake = _games[i].snakeBody,
                gameOver = _games[i].gameOver
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
        
        await Task.Delay(170);
    }

}