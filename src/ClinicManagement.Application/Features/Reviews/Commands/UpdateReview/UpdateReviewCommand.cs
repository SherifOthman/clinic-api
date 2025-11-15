using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Reviews.Commands.UpdateReview;

public record UpdateReviewCommand : IRequest<Result<ReviewDto>>
{
    public int Id { get; set; }
    public string Quote { get; set; } = string.Empty;
    public int Rating { get; set; }
}
