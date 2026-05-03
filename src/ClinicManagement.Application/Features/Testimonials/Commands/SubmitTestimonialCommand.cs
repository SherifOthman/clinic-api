using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

/// <summary>
/// Only the review content — AuthorName, Position, ClinicName, and AvatarUrl
/// are resolved from the User and Clinic tables in the handler.
/// This prevents spoofing and keeps the data consistent with the source of truth.
/// </summary>
public record SubmitTestimonialCommand(
    string Text,
    int Rating
) : IRequest<Result>;
