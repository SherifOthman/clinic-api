using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A review/testimonial submitted by a ClinicOwner, shown on the public marketing website.
/// </summary>
public class Testimonial : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Display name of the reviewer (e.g. "Dr. Sarah Johnson").</summary>
    public string AuthorName { get; set; } = null!;

    /// <summary>Job title / role (e.g. "Clinic Administrator").</summary>
    public string Position { get; set; } = null!;

    /// <summary>Clinic name shown under the reviewer.</summary>
    public string ClinicName { get; set; } = null!;

    /// <summary>The review text.</summary>
    public string Text { get; set; } = null!;

    /// <summary>Star rating 1–5.</summary>
    public int Rating { get; set; }

    /// <summary>Optional profile image URL.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Only approved testimonials are shown publicly.</summary>
    public bool IsApproved { get; set; } = false;

    // Navigation
    public Clinic Clinic { get; set; } = null!;
    public User User { get; set; } = null!;
}
