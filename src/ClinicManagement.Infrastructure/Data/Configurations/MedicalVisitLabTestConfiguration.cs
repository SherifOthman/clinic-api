using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicalVisitLabTestConfiguration : IEntityTypeConfiguration<MedicalVisitLabTest>
{
    public void Configure(EntityTypeBuilder<MedicalVisitLabTest> builder)
    {
        builder.HasKey(mvlt => mvlt.Id);

        builder.HasOne(mvlt => mvlt.MedicalVisit)
            .WithMany(mv => mv.LabTests)
            .HasForeignKey(mvlt => mvlt.MedicalVisitId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(mvlt => mvlt.LabTest)
            .WithMany(lt => lt.MedicalVisitLabTests)
            .HasForeignKey(mvlt => mvlt.LabTestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(mvlt => mvlt.Notes)
            .HasMaxLength(1000);
    }
}
