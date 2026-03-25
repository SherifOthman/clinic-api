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
        
        builder.HasOne(cb => cb.Clinic)
            .WithMany(c => c.Branches)
            .HasForeignKey(cb => cb.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(cb => cb.PhoneNumbers)
            .WithOne()
            .HasForeignKey(pn => pn.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(cb => cb.AppointmentPrices)
            .WithOne()
            .HasForeignKey(ap => ap.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
