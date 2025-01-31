using Discord;
using Logliven.Client.Models;
using Logliven.Client.Services;
using Logliven.Common;
using Logliven.Discord;
using Logliven.Infrastructure.Authentication;
using Logliven.Infrastructure.Exceptions;
using Logliven.Postgres;
using Logliven.Postgres.Entities;
using Logliven.Postgres.Extensions;
using Logliven.Services.Discord;
using Microsoft.EntityFrameworkCore;

namespace Logliven.Services;

public class GuildService(
    DiscordRestClientService discordClientService,
    DiscordBotClient discordBotClient,
    IHttpContextAccessor httpContextAccessor,
    IDbContextFactory<LoglivenDbContext> dbContextFactory) : IGuildService {
    public async Task<IEnumerable<UserGuildView>> GetAvailableGuilds(CancellationToken token = default) {
        var userGuilds = await discordClientService.GetGuildSummaries(token);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
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

    public async Task<GuildCardView> GetGuildCard(ulong guildId, CancellationToken token = default) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        var guildEntity = await dbContext.Guilds
                              .Where(channel => channel.Id == guildId)
                              .Include(entity => entity.Channels)
                              .ThenInclude(entity => entity.Author)
                              .FirstOrDefaultAsync(cancellationToken: token)
                          ?? throw new NotFoundException();

        var userGuilds = await discordClientService.GetGuildSummaries(token);
        if (!userGuilds.Any(userGuild => userGuild.Permissions.Administrator && userGuild.Id == guildId)) {
            throw new ForbidException();
        }

        var guild = await discordBotClient.RetrieveGuild(guildId);
        var guildChannels = await discordBotClient.RetrieveGuildChannels(guildId);

        var channelViews = guildChannels
            .Where(channel => channel.ChannelType == ChannelType.Text)
            .ToDictionary(channel => channel.Id, channel => channel.Name);

        var channelsRestrictions = guildEntity.Channels
            .Select(entity => new GuildChannelRestrictionView(entity.Id, entity.Restrictions,
                new UserView(entity.Author.Id, entity.Author.Discriminator, entity.Author.AvatarUrl)));
        return new GuildCardView(guild.Name, guild.IconUrl, channelViews, channelsRestrictions);
    }

    public async Task AddChannelRestriction(ulong guildId, ulong channelId, RestrictionType restrictionType,
        CancellationToken token = default) {
        var userGuilds = await discordClientService.GetGuildSummaries(token);
        if (!userGuilds.Any(userGuild => userGuild.Permissions.Administrator && userGuild.Id == guildId)) {
            throw new ForbidException();
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
        await dbContext.Guilds
            .Where(g => g.Id == guildId)
            .EnsureExistsAsync(token);
        
        var userInfo = httpContextAccessor.HttpContext!.GetDiscordUserInfoOrThrow();
        var user = await dbContext.Users.FirstOrDefaultAsync(entity => entity.Id == userInfo.Id)
            ?? dbContext.Users.Add(new UserEntity() {
                Id = userInfo.Id,
                Discriminator = userInfo.Username,
                AvatarUrl = userInfo.AvatarUrl,
            }).Entity;


        var restriction = new GuildChannelEntity
        {
            Id = channelId,
            GuildId = guildId,
            Restrictions = restrictionType,
            AuthorId = user.Id,
            Author = user
        };
        
        dbContext.GuildChannels.Add(restriction);
        await dbContext.SaveChangesAsync(token);
    }

    public async Task RemoveChannelRestriction(ulong guildId, ulong channelId, CancellationToken token = default) {
        var userGuilds = await discordClientService.GetGuildSummaries(token);
        if (!userGuilds.Any(userGuild => userGuild.Permissions.Administrator && userGuild.Id == guildId)) {
            throw new ForbidException();
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
        await dbContext.GuildChannels
            .Where(c => c.GuildId == guildId && c.Id == channelId)
            .ExecuteDeleteAsync(token);
    }
}