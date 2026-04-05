using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(50).IsRequired();
        builder.Property(a => a.FullName).HasMaxLength(200);
        builder.Property(a => a.Username).HasMaxLength(100);
        builder.Property(a => a.UserEmail).HasMaxLength(200);
        builder.Property(a => a.UserRole).HasMaxLength(50);
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.Changes).HasColumnType("nvarchar(max)");

        // Indexes for common SuperAdmin queries
        builder.HasIndex(a => new { a.ClinicId, a.Timestamp });
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => new { a.UserId, a.Timestamp });
        builder.HasIndex(a => a.Timestamp);
    }
}
