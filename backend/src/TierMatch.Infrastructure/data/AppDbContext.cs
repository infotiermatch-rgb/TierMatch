using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TierMatch.Domain.Entities;
using TierMatch.Infrastructure.Identity;

namespace TierMatch.Infrastructure.Data;

public class AppDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Animal> Animals => Set<Animal>();

    public DbSet<Shelter> Shelters => Set<Shelter>();

    public DbSet<AnimalImage> AnimalImages => Set<AnimalImage>();

    public DbSet<AdoptionRequest> AdoptionRequests
        => Set<AdoptionRequest>();

        public DbSet<RefreshToken> RefreshTokens
    => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}