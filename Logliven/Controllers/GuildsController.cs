using Discord;
using Discord.WebSocket;
using Logliven.Client.Models;
using Logliven.Client.Services;
using Logliven.Discord;
using Logliven.Infrastructure.Exceptions;
using Logliven.Postgres;
using Logliven.Postgres.Entities;
using Logliven.Services.Discord;
using Logliven.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logliven.Controllers;

[ApiController]
[Route("api/guilds")]
[Authorize]
public class GuildsController : ControllerBase {
    private readonly IGuildService _guildService;

    public GuildsController(IGuildService guildService) {
        _guildService = guildService;
    }

    [HttpGet]
    public Task<IEnumerable<UserGuildView>> GetAvailableGuilds(CancellationToken token)
        => _guildService.GetAvailableGuilds(token);

    [HttpGet("{guildId:long}")]
    public Task<GuildCardView> GetGuildCard(ulong guildId, CancellationToken token)
        => _guildService.GetGuildCard(guildId, token);

    [HttpPost("{guildId:long}/channels/{channelId:long}/restrictions")]
    public Task AddChannelRestriction(ulong guildId, ulong channelId, [FromBody] RestrictionType restrictionType, CancellationToken token)
        => _guildService.AddChannelRestriction(guildId, channelId, restrictionType, token);

    [HttpDelete("{guildId:long}/channels/{channelId:long}/restrictions")]
    public Task RemoveChannelRestriction(ulong guildId, ulong channelId, CancellationToken token)
        => _guildService.RemoveChannelRestriction(guildId, channelId, token);
}