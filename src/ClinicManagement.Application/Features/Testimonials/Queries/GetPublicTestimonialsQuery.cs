using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public record GetPublicTestimonialsQuery : IRequest<List<TestimonialDto>>;

public record TestimonialDto(
    string AuthorName,
    string Position,
    string ClinicName,
    string Text,
    int Rating,
    string? AvatarUrl
);
