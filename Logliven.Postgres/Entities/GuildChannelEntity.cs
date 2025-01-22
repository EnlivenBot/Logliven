namespace Logliven.Postgres.Entities;

public class GuildChannelEntity : IEntityId<ulong> {
    public required ulong Id { get; set; }
    public GuildEntity Guild { get; set; } = null!;
    public required ulong GuildId { get; set; }
    public RestrictionType Restrictions { get; set; }
    public UserEntity Author { get; set; } = null!;
    public required ulong AuthorId { get; set; }
}