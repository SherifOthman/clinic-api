using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Reviews.Commands.UpdateReview;

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReviewCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewDto>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.Id, cancellationToken);
        
        if (review == null)
            return Result<ReviewDto>.Fail("Review not found");

        review.Quote = request.Quote;
        review.Rating = request.Rating;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load navigation properties
        var user = await _unitOfWork.Users.GetByIdAsync(review.UserId, cancellationToken);
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(review.ClinicId, cancellationToken);

        var reviewDto = new ReviewDto
        {
            Id = review.Id,
            UserName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
            UserAvatar = user?.Avatar,
            ClinicName = clinic?.Name ?? string.Empty,
            Quote = review.Quote,
            Rating = review.Rating
        };

        return Result<ReviewDto>.Ok(reviewDto);
    }
}
