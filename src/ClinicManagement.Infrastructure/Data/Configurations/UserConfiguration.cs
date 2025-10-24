using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SecondName).HasMaxLength(100);
        builder.Property(e => e.ThirdName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Avatar).HasMaxLength(200);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.ToTable("Users");
    }
}

