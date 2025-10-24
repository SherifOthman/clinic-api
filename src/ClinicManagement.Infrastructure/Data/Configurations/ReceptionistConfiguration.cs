using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ReceptionistConfiguration : IEntityTypeConfiguration<Receptionist>
{
    public void Configure(EntityTypeBuilder<Receptionist> builder)
    {
        builder.ToTable("Receptionists");
        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Branch)
            .WithMany(p => p.Receptionists)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

