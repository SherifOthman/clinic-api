using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
{
    public void Configure(EntityTypeBuilder<EmailQueue> builder)
    {
        builder.Property(eq => eq.ToEmail).HasMaxLength(256).IsRequired();
        builder.Property(eq => eq.ToName).HasMaxLength(200);
        builder.Property(eq => eq.Subject).HasMaxLength(500).IsRequired();
        builder.Property(eq => eq.ErrorMessage).HasMaxLength(1000);
    }
}
