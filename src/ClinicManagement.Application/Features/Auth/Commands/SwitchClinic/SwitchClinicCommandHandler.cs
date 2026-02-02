using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.SwitchClinic;

public class SwitchClinicCommandHandler : IRequestHandler<SwitchClinicCommand, Result<SwitchClinicResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenService _tokenService;
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<SwitchClinicCommandHandler> _logger;

    public SwitchClinicCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ITokenService tokenService,
        IUserManagementService userManagementService,
        ILogger<SwitchClinicCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _tokenService = tokenService;
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<Result<SwitchClinicResponse>> Handle(SwitchClinicCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        // Check if user has access to the requested clinic
        var userClinic = await _unitOfWork.UserClinics.GetUserClinicWithDetailsAsync(userId, request.ClinicId, cancellationToken);

        if (userClinic == null)
        {
            _logger.LogWarning("User {UserId} attempted to switch to unauthorized clinic {ClinicId}", userId, request.ClinicId);
            return Result<SwitchClinicResponse>.Fail(MessageCodes.Authentication.UNAUTHORIZED_ACCESS);
        }

        // Get the user entity
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result<SwitchClinicResponse>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        // Update user's current clinic
        user.CurrentClinicId = request.ClinicId;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate new access token with the new clinic context
        var userRoles = await _userManagementService.GetUserRolesAsync(user, cancellationToken);
        var newAccessToken = _tokenService.GenerateAccessToken(user, userRoles, request.ClinicId);

        var currentClinicDto = userClinic.Adapt<UserClinicDto>();
        currentClinicDto.IsCurrent = true;

        var response = new SwitchClinicResponse
        {
            AccessToken = newAccessToken,
            CurrentClinic = currentClinicDto
        };

        _logger.LogInformation("User {UserId} switched to clinic {ClinicId}", userId, request.ClinicId);

        return Result<SwitchClinicResponse>.Ok(response);
    }
}