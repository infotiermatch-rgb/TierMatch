using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TierMatch.Domain.Entities;

namespace TierMatch.Infrastructure.Data.Configurations;

public class AdoptionRequestConfiguration
    : IEntityTypeConfiguration<AdoptionRequest>
{
    public void Configure(EntityTypeBuilder<AdoptionRequest> builder)
    {
        builder.ToTable("AdoptionRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Message)
            .HasMaxLength(4000);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.RequestedAt)
            .IsRequired();

        builder.HasOne(x => x.Animal)
            .WithMany(x => x.AdoptionRequests)
            .HasForeignKey(x => x.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}