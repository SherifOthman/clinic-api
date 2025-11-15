using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Reviews.Queries.GetReviewById;

public record GetReviewByIdQuery : IRequest<Result<ReviewDto>>
{
    public int Id { get; set; }
}
