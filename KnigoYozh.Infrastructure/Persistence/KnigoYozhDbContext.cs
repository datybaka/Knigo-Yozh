using KnigoYozh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnigoYozh.Infrastructure.Persistence;

public class KnigoYozhDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public KnigoYozhDbContext(DbContextOptions<KnigoYozhDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KnigoYozhDbContext).Assembly);
    }
}