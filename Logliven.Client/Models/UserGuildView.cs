namespace Logliven.Client.Models;

public record UserGuildView(ulong Id, string Name, string IconUrl, int? MembersCount, bool BotJoined);