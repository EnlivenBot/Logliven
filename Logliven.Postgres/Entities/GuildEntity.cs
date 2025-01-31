namespace Logliven.Postgres.Entities;

public class GuildEntity : IEntityId<ulong> {
    public required ulong Id { get; set; }
    public required string Name { get; set; }
    public required string IconUrl { get; set; }
    public virtual HashSet<GuildChannelEntity> Channels { get; } = [];
}