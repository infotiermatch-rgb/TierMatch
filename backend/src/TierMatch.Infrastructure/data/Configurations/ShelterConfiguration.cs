using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierMatch.Domain.Entities;

namespace TierMatch.Infrastructure.Persistence.Configurations;

public class ShelterConfiguration : IEntityTypeConfiguration<Shelter>
{
    public void Configure(EntityTypeBuilder<Shelter> builder)
    {
        builder.ToTable("Shelters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Street)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.HouseNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.PostalCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.Website)
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.HasMany(x => x.Animals)
            .WithOne(x => x.Shelter)
            .HasForeignKey(x => x.ShelterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}