using Logliven.Client.Models;
using Logliven.Postgres;
using System.Net.Http.Json;

namespace Logliven.Client.Services;

public class HttpGuildService : IGuildService
{
    private readonly HttpClient _httpClient;

    public HttpGuildService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<UserGuildView>> GetAvailableGuilds(CancellationToken token = default)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<UserGuildView>>("api/guilds", token)
            ?? throw new InvalidOperationException("Failed to retrieve guilds");
    }

    public async Task<GuildCardView> GetGuildCard(ulong guildId, CancellationToken token = default)
    {
        return await _httpClient.GetFromJsonAsync<GuildCardView>($"api/guilds/{guildId}", token)
            ?? throw new InvalidOperationException("Failed to retrieve guild card");
    }

    public async Task AddChannelRestriction(ulong guildId, ulong channelId, RestrictionType restrictionType, CancellationToken token = default)
    {
        await _httpClient.PostAsJsonAsync($"api/guilds/{guildId}/channels/{channelId}/restrictions", restrictionType, token);
    }

    public async Task RemoveChannelRestriction(ulong guildId, ulong channelId, CancellationToken token = default)
    {
        await _httpClient.DeleteAsync($"api/guilds/{guildId}/channels/{channelId}/restrictions", token);
    }
} 