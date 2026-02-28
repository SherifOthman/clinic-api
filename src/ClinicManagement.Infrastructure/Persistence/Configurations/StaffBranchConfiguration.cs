using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class StaffBranchConfiguration : IEntityTypeConfiguration<StaffBranch>
{
    public void Configure(EntityTypeBuilder<StaffBranch> builder)
    {
        builder.HasOne<Staff>()
            .WithMany(s => s.StaffBranches)
            .HasForeignKey(sb => sb.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne<ClinicBranch>()
            .WithMany()
            .HasForeignKey(sb => sb.ClinicBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
