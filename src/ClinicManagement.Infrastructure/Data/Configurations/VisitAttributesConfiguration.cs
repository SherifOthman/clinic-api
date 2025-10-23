using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class VisitAttributesConfiguration : IEntityTypeConfiguration<VisitAttributes>
{
    public void Configure(EntityTypeBuilder<VisitAttributes> builder)
    {
        builder.ToTable("VisitAttributes");
        builder.Property(e => e.Name).HasMaxLength(100);
        builder.Property(e => e.Label).HasMaxLength(100);
        builder.Property(e => e.Type).HasMaxLength(30);
    }
}

