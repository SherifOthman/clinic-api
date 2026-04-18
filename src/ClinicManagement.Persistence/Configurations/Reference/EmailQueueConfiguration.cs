using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
{
    public void Configure(EntityTypeBuilder<EmailQueue> builder)
    {
        builder.Property(eq => eq.ToEmail).HasMaxLength(256).IsRequired();
        builder.Property(eq => eq.ToName).HasMaxLength(200);
        builder.Property(eq => eq.Subject).HasMaxLength(500).IsRequired();
        builder.Property(eq => eq.ErrorMessage).HasMaxLength(1000);
        builder.Property(eq => eq.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_EmailQueue_Status",
            "[Status] IN ('Pending', 'Sending', 'Sent', 'Failed')"));

        builder.HasIndex(eq => new { eq.Status, eq.ScheduledFor });
    }
}
