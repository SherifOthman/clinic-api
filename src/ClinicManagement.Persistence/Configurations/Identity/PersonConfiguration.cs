using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.ProfileImageUrl).HasMaxLength(500);
        builder.Property(p => p.Gender).HasConversion<short>().HasColumnType("smallint");

        // Gender must be a valid enum value (0=Male, 1=Female)
        builder.ToTable(t => t.HasCheckConstraint("CK_Person_Gender", "[Gender] IN (0, 1)"));
    }
}
