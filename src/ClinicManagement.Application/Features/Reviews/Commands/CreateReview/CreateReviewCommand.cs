using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Reviews.Commands.CreateReview;

public record CreateReviewCommand : IRequest<Result<ReviewDto>>
{
    public int UserId { get; set; }
    public int ClinicId { get; set; }
    public string Quote { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
}
