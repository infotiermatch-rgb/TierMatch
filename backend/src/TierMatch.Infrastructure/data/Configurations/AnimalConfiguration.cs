using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierMatch.Domain.Entities;

namespace TierMatch.Infrastructure.Data.Configurations;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.ToTable("Animals");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Breed)
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.Species)
            .HasConversion<int>();

        builder.Property(a => a.Gender)
            .HasConversion<int>();

        builder.Property(a => a.Size)
            .HasConversion<int>();
    }
}