using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class SpecializationAttributeConfiguration : IEntityTypeConfiguration<SpecializationAttribute>
{
    public void Configure(EntityTypeBuilder<SpecializationAttribute> builder)
    {
        builder.ToTable("SpecializationAttributes");
        builder.HasOne(d => d.Specialization)
            .WithMany(p => p.SpecializationAttributes)
            .HasForeignKey(d => d.SpecializationId);
        builder.HasOne(d => d.VisitAttribute)
            .WithMany(p => p.SpecializationAttributes)
            .HasForeignKey(d => d.VisitAttributeId);
    }
}

