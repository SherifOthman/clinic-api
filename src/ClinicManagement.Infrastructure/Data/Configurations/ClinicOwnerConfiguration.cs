using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicOwnerConfiguration : IEntityTypeConfiguration<ClinicOwner>
{
    public void Configure(EntityTypeBuilder<ClinicOwner> builder)
    {
        builder.ToTable("ClinicOwners");

        builder.HasKey(co => co.Id);

        builder.Property(co => co.UserId)
            .IsRequired();

        builder.Property(co => co.BusinessLicense)
            .HasMaxLength(100);

        builder.Property(co => co.TaxId)
            .HasMaxLength(50);

        builder.Property(co => co.OwnershipPercentage)
            .HasColumnType("decimal(5,2)");

        // One-to-one relationship with User
        builder.HasOne(co => co.User)
            .WithOne(u => u.ClinicOwner)
            .HasForeignKey<ClinicOwner>(co => co.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(co => co.UserId)
            .IsUnique();
    }
}
