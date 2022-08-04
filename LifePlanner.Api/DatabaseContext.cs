using LifePlanner.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Api;

public class DatabaseContext : DbContext
{
    public DbSet<Activity> Activities { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        
    }
}