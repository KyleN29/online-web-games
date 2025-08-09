using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

public class WebSocketController : ControllerBase
{
    [Route("/ws")]
    public string Get()
    {
        return "Hello World";
    }
}