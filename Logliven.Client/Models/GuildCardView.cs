namespace Logliven.Client.Models;

public record GuildCardView(string Name, string? IconUrl, IDictionary<ulong, string> Channels, IEnumerable<GuildChannelRestrictionView> Restrictions);