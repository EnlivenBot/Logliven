namespace Logliven.Infrastructure.Authentication;

public record DiscordAuthedUserInfo(ulong Id, string Username, string AvatarHash) {
    public string AvatarUrl => $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png";
}