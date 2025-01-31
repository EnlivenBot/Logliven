namespace Logliven.Client.Models;

public record UserView(ulong UserId, string Discriminator, string? AvatarUrl);