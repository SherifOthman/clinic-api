using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Name fields configuration
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();    
        
        // Other properties
        builder.Property(e => e.Avatar).HasMaxLength(200).IsUnicode(false).IsRequired();
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        
        builder.ToTable("Users");
    }
}

