using System.Collections.Concurrent;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;

namespace Logliven.Discord;

public class DiscordBotClient(DiscordSocketConfig config, IMemoryCache memoryCache) : DiscordSocketClient(config) {
    public async Task<IGuild> RetrieveGuild(ulong guildId) {
        var socketGuild = GetGuild(guildId);
        if (socketGuild is not null) {
            return socketGuild;
        }
        
        var key = $"{nameof(RetrieveGuild)}-{guildId})";
        return await memoryCache.GetOrCreate(key, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
            return await Rest.GetGuildAsync(guildId);
        })!;
    }
    
    public async Task<IGuildUser> RetrieveGuildUser(ulong guildId, ulong userId) {
        var socketGuildUser = GetGuild(guildId)?.GetUser(userId);
        if (socketGuildUser is not null) {
            return socketGuildUser;
        }
        var key = $"{nameof(RetrieveGuildUser)}-{guildId}-{userId})";
        return await memoryCache.GetOrCreate(key, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            return await Rest.GetGuildUserAsync(guildId, userId);
        })!;
    }
    
    public async Task<IReadOnlyCollection<IGuildChannel>> RetrieveGuildChannels(ulong guildId) {
        var socketGuild = GetGuild(guildId);
        if (socketGuild is not null) {
            return socketGuild.Channels;
        }
        
        var key = $"{nameof(RetrieveGuildChannels)}-{guildId})";
        return await memoryCache.GetOrCreate(key, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            var guild = await RetrieveGuild(guildId);
            return await guild.GetChannelsAsync();
        })!;
    }
}