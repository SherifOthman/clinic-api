using ClinicManagement.Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(o => o.OccurredAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(o => o.ProcessedAt)
            .HasColumnType("datetime2");

        builder.Property(o => o.Error)
            .HasColumnType("nvarchar(max)");

        builder.Property(o => o.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes for efficient querying
        builder.HasIndex(o => o.ProcessedAt)
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt");

        builder.HasIndex(o => o.OccurredAt)
            .HasDatabaseName("IX_OutboxMessages_OccurredAt");

        builder.HasIndex(o => new { o.ProcessedAt, o.RetryCount })
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt_RetryCount");
    }
}
