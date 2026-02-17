using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicalVisitRadiologyConfiguration : IEntityTypeConfiguration<MedicalVisitRadiology>
{
    public void Configure(EntityTypeBuilder<MedicalVisitRadiology> builder)
    {
        builder.HasKey(mvr => mvr.Id);

        builder.HasOne(mvr => mvr.MedicalVisit)
            .WithMany(mv => mv.RadiologyTests)
            .HasForeignKey(mvr => mvr.MedicalVisitId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(mvr => mvr.RadiologyTest)
            .WithMany(rt => rt.MedicalVisitRadiologies)
            .HasForeignKey(mvr => mvr.RadiologyTestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(mvr => mvr.Notes)
            .HasMaxLength(500);
    }
}
