using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public record GetAllTestimonialsQuery : IRequest<List<AdminTestimonialDto>>;

public record AdminTestimonialDto(
    Guid Id,
    string AuthorName,
    string Position,
    string ClinicName,
    string Text,
    int Rating,
    string? AvatarUrl,
    bool IsApproved,
    DateTimeOffset CreatedAt
);
