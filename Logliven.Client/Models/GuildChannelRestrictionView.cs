using Logliven.Postgres;

namespace Logliven.Client.Models;

public record GuildChannelRestrictionView(ulong ChannelId, RestrictionType Type, UserView Author);