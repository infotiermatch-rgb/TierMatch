using Microsoft.EntityFrameworkCore;
using TierMatch.Domain.Entities;

namespace TierMatch.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Animal> Animals => Set<Animal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    public DbSet<Shelter> Shelters => Set<Shelter>();

    public DbSet<AnimalImage> AnimalImages => Set<AnimalImage>();

    public DbSet<AdoptionRequest> AdoptionRequests
    => Set<AdoptionRequest>();
}