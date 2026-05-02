using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class ClinicMemberPermissionConfiguration : IEntityTypeConfiguration<ClinicMemberPermission>
{
    public void Configure(EntityTypeBuilder<ClinicMemberPermission> builder)
    {
        builder.Property(p => p.Permission).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_ClinicMemberPermission_Permission",
            "[Permission] IN ('ViewPatients','CreatePatient','EditPatient','DeletePatient'," +
            "'ViewStaff','InviteStaff','ManageStaffStatus'," +
            "'ViewBranches','ManageBranches'," +
            "'ManageSchedule','ManageVisitTypes'," +
            "'ViewAppointments','ManageAppointments'," +
            "'ViewInvoices','ManageInvoices')"));

        // One row per permission per member
        builder.HasIndex(p => new { p.ClinicMemberId, p.Permission }).IsUnique();

        builder.HasOne(p => p.ClinicMember)
            .WithMany(m => m.Permissions)
            .HasForeignKey(p => p.ClinicMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
