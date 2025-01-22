namespace Logliven.Postgres.Entities;

public class UserEntity : IEntityId<ulong> {
    public required ulong Id { get; set; }
    public required string Discriminator { get; set; }
    public string? AvatarUrl { get; set; }
}