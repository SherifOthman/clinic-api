using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.ProfileImageUrl).HasMaxLength(500);
        builder.Property(u => u.Gender).HasConversion<string>().HasMaxLength(10).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_User_Gender", "[Gender] IN ('Male', 'Female')"));
    }
}
