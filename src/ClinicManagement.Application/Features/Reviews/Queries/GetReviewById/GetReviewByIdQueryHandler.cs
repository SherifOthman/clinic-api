using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Reviews.Queries.GetReviewById;

public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, Result<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReviewByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        
        if (review == null)
            return Result<ReviewDto>.Fail("Review not found");

        var reviewDto = new ReviewDto
        {
            Id = review.Id,
            UserName = review.User != null ? $"{review.User.FirstName} {review.User.LastName}" : string.Empty,
            UserAvatar = review.User?.Avatar,
            ClinicName = review.Clinic?.Name ?? string.Empty,
            Quote = review.Quote,
            Rating = review.Rating
        };

        return Result<ReviewDto>.Ok(reviewDto);
    }
}
