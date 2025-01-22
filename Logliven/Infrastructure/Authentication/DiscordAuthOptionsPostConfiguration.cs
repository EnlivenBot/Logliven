using Discord.OAuth2;
using Microsoft.Extensions.Options;

namespace Logliven.Infrastructure.Authentication;

public class DiscordAuthOptionsPostConfiguration(IConfiguration configuration) : IPostConfigureOptions<DiscordOptions>  {
    public void PostConfigure(string? name, DiscordOptions options) {
        options.AppId = configuration["Discord:AppId"];
        options.AppSecret = configuration["Discord:AppSecret"];

        options.Scope.Add("guilds");
        options.SaveTokens = true;
    }
}