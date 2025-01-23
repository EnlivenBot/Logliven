using Discord;
using Discord.Rest;
using Microsoft.Extensions.Caching.Memory;

namespace Logliven.Services.Discord;

public class DiscordRestGuildProvider(DiscordRestClientAccessor clientAccessor, IMemoryCache memoryCache) {
    public async Task<IEnumerable<RestUserGuild>> GetGuildSummaries(CancellationToken token) {
        var restClient = await clientAccessor.GetRestClientFromAccessToken();
        
        var guildTask = memoryCache.GetOrCreate(restClient.CurrentUser,
            async entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                entry.Dispose();
                try
                {
                    return await restClient.GetGuildSummariesAsync().FlattenAsync();
                }
                catch (Exception)
                {
                    // Resetting entry
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.Zero;
                    throw;
                }
            });
        
        return await guildTask!;
    }
}