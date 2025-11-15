using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateReviewCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = new Review
        {
            UserId = request.UserId,
            ClinicId = request.ClinicId,
            Quote = request.Quote,
            Rating = request.Rating
        };

        _unitOfWork.Reviews.Add(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdReview = await _unitOfWork.Reviews.GetByIdWithDetailsAsync(review.Id, cancellationToken);
        
        if (createdReview == null)
            return Result<ReviewDto>.Fail("Failed to create review");

        var reviewDto = new ReviewDto
        {
            Id = createdReview.Id,
            UserName = createdReview.User != null ? $"{createdReview.User.FirstName} {createdReview.User.LastName}" : string.Empty,
            UserAvatar = createdReview.User?.Avatar,
            ClinicName = createdReview.Clinic?.Name ?? string.Empty,
            Quote = createdReview.Quote,
            Rating = createdReview.Rating
        };

        return Result<ReviewDto>.Ok(reviewDto);
    }
}
