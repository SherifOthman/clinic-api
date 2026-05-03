using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.Testimonials;

/// <summary>
/// Shared mapping helpers for Testimonial → DTO conversions.
/// AuthorName, Position, AvatarUrl, and ClinicName are always resolved
/// from navigation properties at query time — never stored on the entity.
/// </summary>
internal static class TestimonialMapping
{
    // Position is always "Clinic Owner" — testimonials can only be submitted by owners.
    private const string OwnerPosition = "Clinic Owner";

    public static Queries.TestimonialDto ToPublicDto(Testimonial t) => new(
        AuthorName: t.User.FullName,
        Position:   OwnerPosition,
        ClinicName: t.Clinic.Name,
        Text:       t.Text,
        Rating:     t.Rating,
        AvatarUrl:  t.User.ProfileImageUrl);

    public static Queries.AdminTestimonialDto ToAdminDto(Testimonial t) => new(
        Id:         t.Id,
        AuthorName: t.User.FullName,
        Position:   OwnerPosition,
        ClinicName: t.Clinic.Name,
        Text:       t.Text,
        Rating:     t.Rating,
        AvatarUrl:  t.User.ProfileImageUrl,
        IsApproved: t.IsApproved,
        CreatedAt:  t.CreatedAt);

    public static Queries.MyTestimonialDto ToMyDto(Testimonial t) => new(
        AuthorName: t.User.FullName,
        Position:   OwnerPosition,
        Text:       t.Text,
        Rating:     t.Rating,
        AvatarUrl:  t.User.ProfileImageUrl,
        IsApproved: t.IsApproved);
}
