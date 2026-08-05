using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TierMatch.Domain.Entities;

namespace TierMatch.Infrastructure.Data.Configurations;

public class AdoptionRequestConfiguration
    : IEntityTypeConfiguration<AdoptionRequest>
{
    public void Configure(
        EntityTypeBuilder<AdoptionRequest> builder)
    {
        builder.ToTable("AdoptionRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.UserId)
            .IsRequired(false);

        builder.Property(request => request.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(request => request.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(request => request.Message)
            .HasMaxLength(4000);

        builder.Property(request => request.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(request => request.RequestedAt)
            .IsRequired();

        builder.HasIndex(request => request.UserId);

        builder.HasIndex(
            request => new
            {
                request.UserId,
                request.AnimalId,
                request.Status
            });

        builder.HasOne(request => request.Animal)
            .WithMany(animal => animal.AdoptionRequests)
            .HasForeignKey(request => request.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}