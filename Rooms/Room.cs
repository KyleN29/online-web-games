using System.Drawing;
using System.Net.WebSockets;
using System.Threading.Tasks;

public class Room
{
    public string GameId;
    private List<WebSocket> _clients = new();
    private List<SnakeGame> _games = new();
    private Dictionary<WebSocket, bool> _readyStates = new();
    private int seed;

    public Room(string gameId, int gridSize)
    {
        GameId = gameId;
        var rand = new Random();
        seed = rand.Next();
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
        _games.Add(new SnakeGame(12, _clients.Count - 1, this, seed));
        _readyStates[socket] = false;


        // Let all clients know how many players are currently connected
        var message = new { type = "player_count", playerCount = _clients.Count };
        for (int i = 0; i < _clients.Count; i++)
        {
            await _clients[i].SendJsonAsync(message);
        }

        // _ = Task.Run(() => WaitForReady(socket));

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



    // public async Task WaitForReady(WebSocket socket)
    // {
    //     var buffer = new byte[1024 * 4];

    //     while (_clients.Contains(socket) && socket.State == WebSocketState.Open && !_readyStates[socket])
    //     {
    //         var msg = await socket.ReceiveJsonAsync<ReadyMessage>(buffer);
    //         Console.WriteLine("message...");
    //         if (msg != null && msg.ready)
    //         {
    //             Console.WriteLine("Got a ready message :)");
    //             _readyStates[socket] = true;
    //             var opponentClientIndex = _clients.IndexOf(socket) ^ 1;
    //             var opponentSocket = _clients[opponentClientIndex];
    //             var opponentReady = _readyStates[opponentSocket];


    //             var status = new
    //             {
    //                 type = "ready_status",
    //                 isReady = true,
    //                 opponentReady = opponentReady,
    //             };

    //             await socket.SendJsonAsync(status);

    //             status = new
    //             {
    //                 type = "ready_status",
    //                 isReady = opponentReady,
    //                 opponentReady = true,
    //             };

    //             await opponentSocket.SendJsonAsync(status);
    //             // notify all clients how many are ready

    //             // if all are ready, start game
    //             if (_readyStates.Values.All(v => v) && _clients.Count == 2)
    //             {
    //                 await NotifyRoomReady();
    //                 _ = StartGameLoop();
    //             }
    //         }
    //     }
    // }

    public async Task StartGameLoop()
    {
        Console.WriteLine("Game loop starting...");
        while (_clients.Count == 2) // run as long as 2 clients connected
        {
            int lossCounter = 0; // Counter to check if two players lose in the same step
            int losingPlayerIndex = 0; 
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

                if (state.gameOver)
                {
                    lossCounter++;
                    losingPlayerIndex = i;

                    
                }



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

            // Send game end message since somebody lost and reset variables
            if (lossCounter != 0)
            {
                seed = new Random().Next();
                for (int i = 0; i < 2; i++)
                {
                    int isWinner = Convert.ToInt32(i != losingPlayerIndex);

                    // If it is a tie, signal it by setting is winner to -1
                    if (lossCounter == 2)
                    {
                        isWinner = -1;
                    }

                    var endGameObject = new
                    {
                        type = "end_game",
                        isWinner = isWinner,
                    };
                    byte[] endGameMessage = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(endGameObject);

                    if (_clients[i].State == WebSocketState.Open)
                    {
                        await _clients[i].SendAsync(
                            new ArraySegment<byte>(endGameMessage),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }


                    // Reset ready up and games
                    _readyStates[_clients[i]] = false;

                    _games[i] = new SnakeGame(12, i, this, seed);

                    HandleClient(_clients[i], i);
                }
                return;
            }

            await Task.Delay(170); // ~6 ticks per second
        }
        Console.WriteLine("Game loop ended (not enough clients).");
    }

    public void IncreaseOpponentSnakeLength(int gameNum)
    {
        _games[gameNum].IncreaseLength();
    }



    // public async Task ReceiveDirection(WebSocket webSocket, int gameNum)
    // {
    //     var buffer = new byte[1024 * 4];
    //     var receiveResult = await webSocket.ReceiveJsonAsync<DirectionMessage>(buffer);

    //     // Keep listening as long as the client wants to talk
    //     while (!webSocket.CloseStatus.HasValue)
    //     {

    //         if (receiveResult?.direction != null)
    //         {
    //             var direction = receiveResult.direction;

    //             var curGame = _games[gameNum];
    //             Console.WriteLine("Last Direction: " + curGame.lastDirection.X + "," + curGame.lastDirection.Y + " - Cur Direction: " + curGame.currentDirection.X + "," + curGame.currentDirection.Y);

    //             curGame.SetNewDirection(direction);
    //             if (direction.X != curGame.lastDirection.X && direction.Y != curGame.lastDirection.Y)
    //             {
    //                 curGame.currentDirection = direction;
    //             }

    //         }

    //         receiveResult = await webSocket.ReceiveJsonAsync<DirectionMessage>(buffer);
    //     }
    //     // TODO: Here you need to remove the webSocket from your RoomManager
    // }
    public record BaseMessage
    {
        public string type { get; init; } = string.Empty;
    }

    private record DirectionMessage : BaseMessage
    {
        public SnakeGame.Point direction { get; set; }
    }

    private record ReadyMessage : BaseMessage
    {
        public bool ready { get; set; }
    }
    public class PingMessage
    {
        public string type { get; set; }
        public long timestamp { get; set; } // client’s send time (ms since epoch)
    }
    public async Task HandleClient(WebSocket socket, int playerIndex)
    {
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open)
        {
            var rawJson = await socket.ReceiveStringAsync(buffer);
            Console.WriteLine(rawJson);
            if (rawJson == null) continue;

            var baseMsg = System.Text.Json.JsonSerializer.Deserialize<BaseMessage>(rawJson);
            Console.WriteLine(baseMsg);
            if (baseMsg == null) continue;

            switch (baseMsg.type)
            {
                case "ready":
                    var msg = System.Text.Json.JsonSerializer.Deserialize<ReadyMessage>(rawJson);
                    Console.WriteLine("message...");
                    if (msg != null && msg.ready)
                    {
                        Console.WriteLine("Got a ready message :)");
                        _readyStates[socket] = true;
                        var opponentClientIndex = _clients.IndexOf(socket) ^ 1;
                        var opponentSocket = _clients[opponentClientIndex];
                        var opponentReady = _readyStates[opponentSocket];


                        var status = new
                        {
                            type = "ready_status",
                            isReady = true,
                            opponentReady = opponentReady,
                        };

                        await socket.SendJsonAsync(status);

                        status = new
                        {
                            type = "ready_status",
                            isReady = opponentReady,
                            opponentReady = true,
                        };

                        await opponentSocket.SendJsonAsync(status);
                        // notify all clients how many are ready

                        // if all are ready, start game
                        if (_readyStates.Values.All(v => v) && _clients.Count == 2)
                        {
                            await NotifyRoomReady();
                            _ = StartGameLoop();
                        }
                    }
                    break;

                case "direction":
                    var receiveResult = System.Text.Json.JsonSerializer.Deserialize<DirectionMessage>(rawJson);
                    if (receiveResult?.direction != null)
                    {
                        var direction = receiveResult.direction;

                        var curGame = _games[playerIndex];
                        Console.WriteLine("Last Direction: " + curGame.lastDirection.X + "," + curGame.lastDirection.Y + " - Cur Direction: " + curGame.currentDirection.X + "," + curGame.currentDirection.Y);

                        curGame.SetNewDirection(direction);
                        if (direction.X != curGame.lastDirection.X && direction.Y != curGame.lastDirection.Y)
                        {
                            curGame.currentDirection = direction;
                        }

                    }
                    break;
                case "ping":
                    var pingMsg = System.Text.Json.JsonSerializer.Deserialize<PingMessage>(rawJson);
                    if (pingMsg != null)
                    {
                        var pong = new
                        {
                            type = "pong",
                            // echo client timestamp back so it can compute RTT
                            clientTimestamp = pingMsg.timestamp,
                            serverTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        };

                        await socket.SendJsonAsync(pong);
                    }
                    break;
            }
        }

        Console.WriteLine($"Socket closed for player {playerIndex}");
    }



}