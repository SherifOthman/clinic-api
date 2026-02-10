using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class AppointmentTypeConfiguration : IEntityTypeConfiguration<AppointmentType>
{
    public void Configure(EntityTypeBuilder<AppointmentType> builder)
    {
        builder.ToTable("AppointmentTypes");

        builder.HasKey(at => at.Id);

        // Properties
        builder.Property(at => at.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(at => at.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(at => at.DescriptionEn)
            .HasMaxLength(500);

        builder.Property(at => at.DescriptionAr)
            .HasMaxLength(500);

        builder.Property(at => at.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(at => at.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(at => at.ColorCode)
            .HasMaxLength(7); // For hex color codes like #FF5733

        // Indexes
        builder.HasIndex(at => at.NameEn);
        builder.HasIndex(at => at.NameAr);
        builder.HasIndex(at => at.IsActive);
        builder.HasIndex(at => at.DisplayOrder);

        // Seed data for common appointment types
        builder.HasData(
            new AppointmentType
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                NameEn = "Consultation",
                NameAr = "كشف",
                DescriptionEn = "First time consultation with the doctor",
                DescriptionAr = "كشف أول مرة مع الطبيب",
                IsActive = true,
                DisplayOrder = 1,
                ColorCode = "#4CAF50"
            },
            new AppointmentType
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                NameEn = "Follow-up",
                NameAr = "إعادة",
                DescriptionEn = "Follow-up visit for existing patients",
                DescriptionAr = "زيارة متابعة للمرضى الحاليين",
                IsActive = true,
                DisplayOrder = 2,
                ColorCode = "#2196F3"
            },
            new AppointmentType
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                NameEn = "Therapy Session",
                NameAr = "جلسة",
                DescriptionEn = "Therapy or treatment session",
                DescriptionAr = "جلسة علاج أو معالجة",
                IsActive = true,
                DisplayOrder = 3,
                ColorCode = "#FF9800"
            }
        );
    }
}
