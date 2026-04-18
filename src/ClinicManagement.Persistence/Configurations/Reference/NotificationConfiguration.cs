using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.ActionUrl).HasMaxLength(500);
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        builder.ToTable(t => t.HasCheckConstraint("CK_Notification_Type",
            "[Type] IN ('Info', 'Warning', 'Error', 'Success')"));
    }
}
