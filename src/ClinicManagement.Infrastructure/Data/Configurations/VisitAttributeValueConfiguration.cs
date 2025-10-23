using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class VisitAttributeValueConfiguration : IEntityTypeConfiguration<VisitAttributeValue>
{
    public void Configure(EntityTypeBuilder<VisitAttributeValue> builder)
    {
        builder.ToTable("VisitAttributeValues");
        builder.Property(e => e.Value).HasColumnType("nvarchar(max)");
        builder.HasOne(d => d.Visit)
            .WithMany(p => p.VisitAttributeValues)
            .HasForeignKey(d => d.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Field)
            .WithMany(p => p.VisitAttributeValues)
            .HasForeignKey(d => d.FieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

