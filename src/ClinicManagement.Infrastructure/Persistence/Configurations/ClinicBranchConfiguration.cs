using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class ClinicBranchConfiguration : IEntityTypeConfiguration<ClinicBranch>
{
    public void Configure(EntityTypeBuilder<ClinicBranch> builder)
    {
        builder.Property(cb => cb.Name).HasMaxLength(200).IsRequired();
        builder.Property(cb => cb.AddressLine).HasMaxLength(500).IsRequired();

        // Clinic is the tenant owner — no nav property on ClinicBranch side
        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(cb => cb.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ClinicBranchPhoneNumber>()
            .WithOne()
            .HasForeignKey(pn => pn.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ClinicBranchAppointmentPrice>()
            .WithOne()
            .HasForeignKey(ap => ap.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
