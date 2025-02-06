using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Logliven.Discord.Commands;
using Logliven.Postgres;
using Logliven.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using SlimMessageBus;

namespace Logliven.Discord.Services;

public partial class DiscordClientMessageReceivedHandlerService(
    IDbContextFactory<LoglivenDbContext> dbContextFactory,
    DiscordBotClient botClient,
    IMemoryCache memoryCache) : IHostedService, IConsumer<ChannelRestrictionsChangedEvent> {
    public Task StartAsync(CancellationToken cancellationToken) {
        botClient.MessageReceived += BotClientOnMessageReceived;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        botClient.MessageReceived -= BotClientOnMessageReceived;

        return Task.CompletedTask;
    }

    private async Task BotClientOnMessageReceived(SocketMessage arg) {
        if (arg is not SocketUserMessage { Channel: IGuildChannel guildChannel } message) {
            return;
        }

        var guild = guildChannel.Guild;

        var guildChannelsDataAsync = await GetGuildChannelsDataAsync(guild.Id);
        if (!guildChannelsDataAsync.TryGetValue(guildChannel.Id, out var guildChannelData)) {
            return;
        }

        if (!IsMessageValid(message, guildChannelData.Restrictions)) {
            // TODO Proper handling
            await message.DeleteAsync();
        }
    }

    private bool IsMessageValid(SocketUserMessage message, RestrictionType restriction) {
        if ((restriction & RestrictionType.AllowImages) != 0 &&
            message.Attachments.Any(attachment => attachment.ContentType.StartsWith("image"))) {
            return true;
        }

        if ((restriction & RestrictionType.AllowAttachments) != 0 && message.Attachments.Count != 0) {
            return true;
        }

        if ((restriction & RestrictionType.AllowLinks) != 0 && LinksRegex.IsMatch(message.Content)) {
            return true;
        }

        return false;
    }

    private async Task<Dictionary<ulong, GuildChannelEntity>> GetGuildChannelsDataAsync(ulong guildId) {
        var key = $"{nameof(DiscordClientMessageReceivedHandlerService)}-{guildId}";
        return await memoryCache.GetOrCreate(key, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.GuildChannels
                .Where(entity => entity.GuildId == guildId)
                .ToDictionaryAsync(entity => entity.Id);
        })!;
    }

    public Task OnHandle(ChannelRestrictionsChangedEvent message, CancellationToken cancellationToken) {
        var key = $"{nameof(DiscordClientMessageReceivedHandlerService)}-{message.Id}";
        memoryCache.Remove(key);

        return Task.CompletedTask;
    }

    [GeneratedRegex(@"https?:\/\/[^\s)]+")]
    private static partial Regex LinksRegex { get; }
}