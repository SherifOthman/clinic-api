using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public record GetMyTestimonialQuery : IRequest<MyTestimonialDto?>;

public record MyTestimonialDto(
    string AuthorName,
    string Position,
    string Text,
    int Rating,
    string? AvatarUrl,
    bool IsApproved
);
