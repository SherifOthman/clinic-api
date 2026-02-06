using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PrescriptionConfiguration : BaseEntityConfiguration<Prescription>
{
    public override void Configure(EntityTypeBuilder<Prescription> builder)
    {
        base.Configure(builder);

        builder.HasOne(p => p.Visit)
            .WithOne(mv => mv.Prescription)
            .HasForeignKey<Prescription>(p => p.VisitId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.Items)
            .WithOne(pi => pi.Prescription)
            .HasForeignKey(pi => pi.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
