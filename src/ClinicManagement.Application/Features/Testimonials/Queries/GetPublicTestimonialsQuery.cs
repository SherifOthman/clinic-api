using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public record GetPublicTestimonialsQuery(int Count = 3) : IRequest<Result<List<TestimonialDto>>>;

public record TestimonialDto(
    string AuthorName,
    string Position,
    string ClinicName,
    string Text,
    int Rating,
    string? AvatarUrl
);
