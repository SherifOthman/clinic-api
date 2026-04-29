using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

public record SubmitTestimonialCommand(
    string AuthorName,
    string Position,
    string Text,
    int Rating,
    string? AvatarUrl
) : IRequest<Result>;
