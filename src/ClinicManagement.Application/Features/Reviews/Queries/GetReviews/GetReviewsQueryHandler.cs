using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Reviews.Queries.GetReviews;

public class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, Result<List<ReviewDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReviewsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Reviews.GetReviewsWithDetailsAsync(cancellationToken);
        
        var reviewDtos = new List<ReviewDto>();
        
        foreach (var r in reviews)
        {
            reviewDtos.Add(new ReviewDto
            {
                Id = r.Id,
                UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : string.Empty,
                UserAvatar = r.User?.Avatar,
                ClinicName = r.Clinic?.Name ?? string.Empty,
                Quote = r.Quote,
                Rating = r.Rating
            });
        }
        
        return Result<List<ReviewDto>>.Ok(reviewDtos);
    }
}
