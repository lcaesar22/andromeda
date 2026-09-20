using Andromeda.Domain;
using Microsoft.EntityFrameworkCore;

namespace Andromeda.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Satellite> Satellites => Set<Satellite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Satellite>(entity =>
        {
            entity.HasIndex(s => s.NoradId).IsUnique();
            entity.Property(s => s.Name).HasMaxLength(200);
            entity.Property(s => s.CountryOfOrigin).HasMaxLength(100);
        });
    }
}