using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("games")]
public class GamesController : ControllerBase
{
    private readonly RoomManager _roomManager;

    public GamesController(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    [HttpPost]
    public IActionResult CreateGame()
    {
        Console.WriteLine("generated game");
        var gameId = _roomManager.GenerateGame();
        return Ok(new { gameId });
    }

}