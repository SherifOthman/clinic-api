using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class DoctorBranchConfiguration : IEntityTypeConfiguration<DoctorBranch>
{
    public void Configure(EntityTypeBuilder<DoctorBranch> builder)
    {
        builder.ToTable("DoctorBranches");
        builder.HasOne(d => d.Doctor)
            .WithMany(p => p.DoctorBranches)
            .HasForeignKey(d => d.DoctorId);

        builder.HasOne(d => d.Branch)
            .WithMany(p => p.DoctorBranches)
            .HasForeignKey(d => d.BranchId);
    }
}

