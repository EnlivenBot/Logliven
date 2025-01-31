using Logliven.Client.Models;
using Logliven.Postgres;

namespace Logliven.Client.Services;

public interface IGuildService
{
    Task<IEnumerable<UserGuildView>> GetAvailableGuilds(CancellationToken token = default);
    Task<GuildCardView> GetGuildCard(ulong guildId, CancellationToken token = default);
    Task AddChannelRestriction(ulong guildId, ulong channelId, RestrictionType restrictionType, CancellationToken token = default);
    Task RemoveChannelRestriction(ulong guildId, ulong channelId, CancellationToken token = default);
} 