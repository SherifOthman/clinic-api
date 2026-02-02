using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetUserClinics;

public class GetUserClinicsQueryHandler : IRequestHandler<GetUserClinicsQuery, Result<IEnumerable<UserClinicDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUserClinicsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<UserClinicDto>>> Handle(GetUserClinicsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        // Get current user to check their CurrentClinicId
        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        // Get user clinics with details
        var userClinics = await _unitOfWork.UserClinics.GetUserClinicsWithDetailsAsync(userId, cancellationToken);

        var userClinicDtos = userClinics.Select(uc => 
        {
            var dto = uc.Adapt<UserClinicDto>();
            dto.IsCurrent = currentUser != null && (currentUser.CurrentClinicId == uc.ClinicId || 
                           (currentUser.CurrentClinicId == null && currentUser.ClinicId == uc.ClinicId));
            return dto;
        });

        return Result<IEnumerable<UserClinicDto>>.Ok(userClinicDtos);
    }
}