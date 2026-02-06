using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class AccountantConfiguration : IEntityTypeConfiguration<Accountant>
{
    public void Configure(EntityTypeBuilder<Accountant> builder)
    {
        builder.ToTable("Accountants");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.CertificationNumber)
            .HasMaxLength(100);

        builder.Property(a => a.MaxApprovalAmount)
            .HasColumnType("decimal(18,2)");

        // One-to-one relationship with User
        builder.HasOne(a => a.User)
            .WithOne(u => u.Accountant)
            .HasForeignKey<Accountant>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.UserId)
            .IsUnique();
    }
}
