using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

public record GetMeQuery : IRequest<Result<UserDto>>;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<UserDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserManagementService _userManagementService;

    public GetMeQueryHandler(
        ICurrentUserService currentUserService,
        IUserManagementService userManagementService)
    {
        _currentUserService = currentUserService;
        _userManagementService = userManagementService;
    }

    public async Task<Result<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<UserDto>.FailSystem("UNAUTHENTICATED", "User is not authenticated");

        var user = await _userManagementService.GetUserByIdAsync(userId.Value, cancellationToken);
        if (user == null)
            return Result<UserDto>.FailSystem("NOT_FOUND", "User not found");

        var userDto = user.Adapt<UserDto>();
        
        // Get user roles
        var userRoles = await _userManagementService.GetUserRolesAsync(user, cancellationToken);
        
        // Set the roles on the DTO
        userDto.Roles = userRoles.ToList();
        
        // Set actual onboarding status from user entity
        userDto.OnboardingCompleted = user.OnboardingCompleted;
        
        return Result<UserDto>.Ok(userDto);
    }
}