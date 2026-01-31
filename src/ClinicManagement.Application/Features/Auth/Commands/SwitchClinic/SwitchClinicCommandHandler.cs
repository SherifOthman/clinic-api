using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.SwitchClinic;

public class SwitchClinicCommandHandler : IRequestHandler<SwitchClinicCommand, Result<SwitchClinicResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenService _tokenService;
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<SwitchClinicCommandHandler> _logger;

    public SwitchClinicCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITokenService tokenService,
        IUserManagementService userManagementService,
        ILogger<SwitchClinicCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _tokenService = tokenService;
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<Result<SwitchClinicResponse>> Handle(SwitchClinicCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        // Check if user has access to the requested clinic
        var userClinic = await _context.UserClinics
            .Include(uc => uc.Clinic)
            .ThenInclude(c => c.SubscriptionPlan)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClinicId == request.ClinicId && uc.IsActive, cancellationToken);

        if (userClinic == null)
        {
            _logger.LogWarning("User {UserId} attempted to switch to unauthorized clinic {ClinicId}", userId, request.ClinicId);
            return Result<SwitchClinicResponse>.Fail(MessageCodes.Authentication.UNAUTHORIZED_ACCESS);
        }

        // Get the user entity
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return Result<SwitchClinicResponse>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        // Update user's current clinic
        user.CurrentClinicId = request.ClinicId;
        await _context.SaveChangesAsync(cancellationToken);

        // Generate new access token with the new clinic context
        var userRoles = await _userManagementService.GetUserRolesAsync(user, cancellationToken);
        var newAccessToken = _tokenService.GenerateAccessToken(user, userRoles, request.ClinicId);

        var response = new SwitchClinicResponse
        {
            AccessToken = newAccessToken,
            CurrentClinic = new UserClinicDto
            {
                Id = userClinic.Id,
                ClinicId = userClinic.ClinicId,
                ClinicName = userClinic.Clinic.Name,
                IsOwner = userClinic.IsOwner,
                IsActive = userClinic.IsActive,
                IsCurrent = true,
                JoinedAt = userClinic.JoinedAt,
                SubscriptionPlan = new SubscriptionPlanDto
                {
                    Id = userClinic.Clinic.SubscriptionPlan.Id,
                    Name = userClinic.Clinic.SubscriptionPlan.Name,
                    Description = userClinic.Clinic.SubscriptionPlan.Description,
                    Price = userClinic.Clinic.SubscriptionPlan.Price,
                    DurationDays = userClinic.Clinic.SubscriptionPlan.DurationDays,
                    MaxUsers = userClinic.Clinic.SubscriptionPlan.MaxUsers,
                    MaxPatients = userClinic.Clinic.SubscriptionPlan.MaxPatients,
                    MaxClinics = userClinic.Clinic.SubscriptionPlan.MaxClinics,
                    MaxBranches = userClinic.Clinic.SubscriptionPlan.MaxBranches,
                    HasAdvancedReporting = userClinic.Clinic.SubscriptionPlan.HasAdvancedReporting,
                    HasApiAccess = userClinic.Clinic.SubscriptionPlan.HasApiAccess,
                    HasPrioritySupport = userClinic.Clinic.SubscriptionPlan.HasPrioritySupport,
                    HasCustomBranding = userClinic.Clinic.SubscriptionPlan.HasCustomBranding,
                    IsActive = userClinic.Clinic.SubscriptionPlan.IsActive
                }
            }
        };

        _logger.LogInformation("User {UserId} switched to clinic {ClinicId}", userId, request.ClinicId);

        return Result<SwitchClinicResponse>.Ok(response);
    }
}