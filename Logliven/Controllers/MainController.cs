using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

namespace Logliven.Controllers;

[Controller]
public class MainController(IConfiguration configuration) : Controller {
    [HttpGet("invite")]
    public IActionResult Index() {
        var appid = configuration["Discord:AppId"];
        var url = $"https://discord.com/oauth2/authorize?client_id={appid}&permissions=8&integration_type=0&scope=bot";
        return Redirect(url);
    }
}