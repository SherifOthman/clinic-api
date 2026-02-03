using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class SpecializationConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.ToTable("Specializations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for performance
        builder.HasIndex(s => s.Name)
            .IsUnique();

        builder.HasIndex(s => s.IsActive);

        // Seed data
        builder.HasData(
            new Specialization { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "General Medicine", Description = "General medical practice", IsActive = true },
            new Specialization { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Cardiology", Description = "Heart and cardiovascular system", IsActive = true },
            new Specialization { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Dermatology", Description = "Skin, hair, and nail conditions", IsActive = true },
            new Specialization { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Pediatrics", Description = "Medical care for infants, children, and adolescents", IsActive = true },
            new Specialization { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Orthopedics", Description = "Musculoskeletal system", IsActive = true },
            new Specialization { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Neurology", Description = "Nervous system disorders", IsActive = true },
            new Specialization { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Psychiatry", Description = "Mental health and behavioral disorders", IsActive = true },
            new Specialization { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Ophthalmology", Description = "Eye and vision care", IsActive = true },
            new Specialization { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "ENT", Description = "Ear, nose, and throat", IsActive = true },
            new Specialization { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Gynecology", Description = "Women's reproductive health", IsActive = true }
        );
    }
}
