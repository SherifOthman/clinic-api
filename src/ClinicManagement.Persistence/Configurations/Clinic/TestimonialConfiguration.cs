using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("Testimonials");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Text).IsRequired().HasMaxLength(1000);
        builder.Property(t => t.Rating).IsRequired();

        builder.HasOne(t => t.Clinic)
            .WithMany()
            .HasForeignKey(t => t.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(t => t.IsApproved);
        builder.HasIndex(t => t.ClinicId).IsUnique(); // one testimonial per clinic
    }
}
