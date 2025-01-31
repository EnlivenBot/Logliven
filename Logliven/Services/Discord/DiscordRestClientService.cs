using Discord;
using Discord.Rest;
using Microsoft.Extensions.Caching.Memory;

namespace Logliven.Services.Discord;

public class DiscordRestClientService(DiscordRestClientAccessor clientAccessor, IMemoryCache memoryCache) {
    private record MemoryCacheKey(RestSelfUser User, string Type);

    public async Task<IEnumerable<RestUserGuild>> GetGuildSummaries(CancellationToken token) {
        return await Process(nameof(GetGuildSummaries),
            async client => await client.GetGuildSummariesAsync().FlattenAsync());
    }

    private async Task<TRet> Process<TRet>(string key, Func<DiscordRestClient, Task<TRet>> getter) {
        var restClient = await clientAccessor.GetRestClientFromAccessToken();

        var memoryCacheKey = new MemoryCacheKey(restClient.CurrentUser, key);
        var task = memoryCache.GetOrCreate(memoryCacheKey,
            async entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                try {
                    return await getter(restClient);
                }
                catch (Exception) {
                    memoryCache.Remove(memoryCacheKey);
                    throw;
                }
            });

        return await task!;
    }
}