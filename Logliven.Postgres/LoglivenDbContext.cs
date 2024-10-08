using Microsoft.EntityFrameworkCore;

namespace Logliven.Postgres;

public class LoglivenDbContext : DbContext
{
    public LoglivenDbContext(DbContextOptions options) : base(options)
    {
    }
    
    
}