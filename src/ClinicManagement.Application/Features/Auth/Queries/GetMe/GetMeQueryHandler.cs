using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserManagementService _userManagementService;

    public GetMeQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IUserManagementService userManagementService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _userManagementService = userManagementService;
    }

    public async Task<Result<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);

        var user = await _userManagementService.GetUserByIdAsync(userId.Value, cancellationToken);
        if (user == null)
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);

        var userDto = user.Adapt<UserDto>();
        
        // Get user roles to determine onboarding completion logic
        var userRoles = await _userManagementService.GetUserRolesAsync(user, cancellationToken);
        
        // Set the roles on the DTO
        userDto.Roles = userRoles.ToList();
        
        // Only ClinicOwners need onboarding - all other roles are considered complete by default
        if (userRoles.Contains("ClinicOwner"))
        {
            // ClinicOwners need to have a ClinicId to be considered onboarding complete
            userDto.OnboardingCompleted = user.ClinicId.HasValue;
        }
        else
        {
            // Admin, Doctor, Receptionist roles don't need onboarding
            userDto.OnboardingCompleted = true;
        }
        
        return Result<UserDto>.Ok(userDto);
    }
}
