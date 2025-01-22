using Discord.WebSocket;
using Logliven.Postgres;
using Logliven.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Logliven.Discord.Services;

internal class DiscordClientGuildListenerService(
    DiscordSocketClient discordClient,
    IDbContextFactory<LoglivenDbContext> dbContextFactory,
    ILogger<DiscordClientGuildListenerService> logger) : BackgroundService {
    public override async Task StartAsync(CancellationToken cancellationToken) {
        await base.StartAsync(cancellationToken);
        discordClient.JoinedGuild += DiscordClientOnJoinedGuild;
        discordClient.GuildAvailable += DiscordClientOnGuildAvailable;
        discordClient.LeftGuild += DiscordClientOnLeftGuild;
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        await base.StopAsync(cancellationToken);
        discordClient.JoinedGuild -= DiscordClientOnJoinedGuild;
        discordClient.GuildAvailable -= DiscordClientOnGuildAvailable;
        discordClient.LeftGuild -= DiscordClientOnLeftGuild;
    }

    private async Task DiscordClientOnGuildAvailable(SocketGuild guild) {
        logger.LogInformation("Guild {Id} {Name} become available", guild.Id, guild.Name);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var entity = await dbContext.Guilds
            .Where(entity => entity.Id == guild.Id)
            .FirstOrDefaultAsync();

        entity ??= dbContext.Guilds.Add(new GuildEntity {
                Id = guild.Id,
                Name = guild.Name,
                IconUrl = guild.IconUrl
            }).Entity;

        entity.Id = guild.Id;
        entity.Name = guild.Name;
        entity.IconUrl = guild.IconUrl;

        await dbContext.SaveChangesAsync();
    }

    private async Task DiscordClientOnJoinedGuild(SocketGuild guild) {
        logger.LogInformation("Bot joined guild {Id} {Name}", guild.Id, guild.Name);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.Guilds.Add(new GuildEntity {
            Id = guild.Id,
            Name = guild.Name,
            IconUrl = guild.IconUrl
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task DiscordClientOnLeftGuild(SocketGuild guild) {
        logger.LogInformation("Bot left guild {Id} {Name}", guild.Id, guild.Name);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Guilds
            .Where(entity => entity.Id == guild.Id)
            .ExecuteDeleteAsync();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        return Task.CompletedTask;
    }
}