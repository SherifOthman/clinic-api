using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Reviews.Queries.GetReviews;

public record GetReviewsQuery : IRequest<Result<List<ReviewDto>>>
{
}
