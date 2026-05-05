using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public record GetAllTestimonialsQuery(int PageNumber = 1, int PageSize = 12)
    : IRequest<Result<PaginatedResult<AdminTestimonialDto>>>;

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
