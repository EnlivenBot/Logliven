using Logliven.Client.Models;
using Logliven.Postgres;
using Logliven.Services.Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logliven.Controllers;

[ApiController]
[Route("api/guilds")]
[Authorize]
public class GuildsController(
    DiscordRestGuildProvider discordGuildProvider,
    IDbContextFactory<LoglivenDbContext> dbContextFactory) : ControllerBase {
    [HttpGet]
    public async Task<IEnumerable<UserGuildView>> GetAvailableGuilds(CancellationToken token) {
        var userGuilds = await discordGuildProvider.GetGuildSummaries(token);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
        var existingGuilds = await dbContext.Guilds
            .Where(entity => userGuilds.Select(guild => guild.Id).Contains(entity.Id))
            .Select(entity => entity.Id)
            .ToArrayAsync(cancellationToken: token);

        return userGuilds
            .Where(guild => guild.Permissions.Administrator)
            .Select(g => {
            var botJoined = existingGuilds.Contains(g.Id);
            return new UserGuildView(g.Id, g.Name, g.IconUrl, g.ApproximateMemberCount, botJoined);
        });
    }
}