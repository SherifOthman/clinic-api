using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

/// <summary>
/// Handler for GetMeQuery that returns current authenticated user.
/// 
/// NOTE: Token refresh is now handled by JwtCookieMiddleware.
/// This handler simply returns the current user using ICurrentUserService.
/// If user reaches this handler, they are already authenticated (middleware handled refresh if needed).
/// </summary>
public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMeQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        _currentUserService.EnsureAuthenticated();
        
        // Get user ID from current user service
        var userId = _currentUserService.GetRequiredUserId();

        // Get user from database
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result<UserDto>.Fail("User not found");
        }

        // Build user response using current user service data
        var userDto = user.Adapt<UserDto>();
        userDto.Roles = _currentUserService.Roles.ToList();
        userDto.ClinicId = _currentUserService.ClinicId;

        return Result<UserDto>.Ok(userDto);
    }
}
