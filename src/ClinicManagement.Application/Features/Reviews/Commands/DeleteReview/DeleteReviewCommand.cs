using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Reviews.Commands.DeleteReview;

public record DeleteReviewCommand : IRequest<Result>
{
    public int Id { get; set; }
}
