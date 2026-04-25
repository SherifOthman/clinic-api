using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.Property(p => p.FullName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.ProfileImageUrl).HasMaxLength(500);
        builder.Property(p => p.Gender).HasConversion<string>().HasMaxLength(10).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_Person_Gender", "[Gender] IN ('Male', 'Female')"));
    }
}
