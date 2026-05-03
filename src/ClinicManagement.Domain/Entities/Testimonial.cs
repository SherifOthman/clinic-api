using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A review/testimonial submitted by a ClinicOwner, shown on the public marketing website.
/// AuthorName, Position, ClinicName, and AvatarUrl are NOT stored here — they are resolved
/// at query time by joining User and Clinic, so they always reflect the current source of truth.
/// </summary>
public class Testimonial : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>The review text.</summary>
    public string Text { get; set; } = null!;

    /// <summary>Star rating 1–5.</summary>
    public int Rating { get; set; }

    /// <summary>Only approved testimonials are shown publicly.</summary>
    public bool IsApproved { get; set; } = false;

    // Navigation
    public Clinic Clinic { get; set; } = null!;
    public User User { get; set; } = null!;
}
