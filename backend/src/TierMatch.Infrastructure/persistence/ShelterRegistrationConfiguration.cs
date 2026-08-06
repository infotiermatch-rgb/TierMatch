using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using TierMatch.Infrastructure.Identity;

namespace TierMatch.Infrastructure.Persistence.Configurations;

public class ShelterRegistrationConfiguration
    : IEntityTypeConfiguration<ShelterRegistration>
{
    public void Configure(
        EntityTypeBuilder<ShelterRegistration> builder)
    {
        builder.ToTable("ShelterRegistrations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShelterName)
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
            .HasMaxLength(2)
            .HasDefaultValue("DE");

        builder.Property(x => x.ShelterPhoneNumber)
            .HasMaxLength(30);

        builder.Property(x => x.ShelterEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Website)
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.ContactFirstName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ContactLastName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ContactEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContactPhoneNumber)
            .HasMaxLength(30);

        builder.Property(x => x.Message)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(
                ShelterRegistrationStatus.Pending);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.ContactEmail);

        builder.HasIndex(x => x.ShelterEmail);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne<Shelter>()
            .WithMany()
            .HasForeignKey(x => x.ShelterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}