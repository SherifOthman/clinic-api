using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.ToTable("Governorates");
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.HasOne(d => d.Country)
            .WithMany(p => p.Governorates)
            .HasForeignKey(d => d.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

