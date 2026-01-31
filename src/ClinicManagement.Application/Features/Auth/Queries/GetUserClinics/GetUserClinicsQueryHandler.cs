using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Auth.Queries.GetUserClinics;

public class GetUserClinicsQueryHandler : IRequestHandler<GetUserClinicsQuery, Result<IEnumerable<UserClinicDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserClinicsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<UserClinicDto>>> Handle(GetUserClinicsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        // Get current user to check their CurrentClinicId
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var userClinics = await _context.UserClinics
            .Include(uc => uc.Clinic)
            .ThenInclude(c => c.SubscriptionPlan)
            .Where(uc => uc.UserId == userId && uc.IsActive)
            .OrderByDescending(uc => uc.IsOwner)
            .ThenBy(uc => uc.Clinic.Name)
            .Select(uc => new UserClinicDto
            {
                Id = uc.Id,
                ClinicId = uc.ClinicId,
                ClinicName = uc.Clinic.Name,
                IsOwner = uc.IsOwner,
                IsActive = uc.IsActive,
                IsCurrent = currentUser != null && (currentUser.CurrentClinicId == uc.ClinicId || 
                           (currentUser.CurrentClinicId == null && currentUser.ClinicId == uc.ClinicId)),
                JoinedAt = uc.JoinedAt,
                SubscriptionPlan = new SubscriptionPlanDto
                {
                    Id = uc.Clinic.SubscriptionPlan.Id,
                    Name = uc.Clinic.SubscriptionPlan.Name,
                    Description = uc.Clinic.SubscriptionPlan.Description,
                    Price = uc.Clinic.SubscriptionPlan.Price,
                    DurationDays = uc.Clinic.SubscriptionPlan.DurationDays,
                    MaxUsers = uc.Clinic.SubscriptionPlan.MaxUsers,
                    MaxPatients = uc.Clinic.SubscriptionPlan.MaxPatients,
                    MaxClinics = uc.Clinic.SubscriptionPlan.MaxClinics,
                    MaxBranches = uc.Clinic.SubscriptionPlan.MaxBranches,
                    HasAdvancedReporting = uc.Clinic.SubscriptionPlan.HasAdvancedReporting,
                    HasApiAccess = uc.Clinic.SubscriptionPlan.HasApiAccess,
                    HasPrioritySupport = uc.Clinic.SubscriptionPlan.HasPrioritySupport,
                    HasCustomBranding = uc.Clinic.SubscriptionPlan.HasCustomBranding,
                    IsActive = uc.Clinic.SubscriptionPlan.IsActive
                }
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<UserClinicDto>>.Ok(userClinics);
    }
}