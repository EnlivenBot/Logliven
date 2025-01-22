using Logliven.Postgres.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logliven.Postgres;

public class LoglivenDbContext : DbContext {
    public LoglivenDbContext() { }

    public LoglivenDbContext(DbContextOptions options) : base(options) { }
    
    public DbSet<GuildChannelEntity> GuildChannels { get; private set; }
    public DbSet<GuildEntity> Guilds { get; private set; }
    public DbSet<UserEntity> Users { get; private set; }
}