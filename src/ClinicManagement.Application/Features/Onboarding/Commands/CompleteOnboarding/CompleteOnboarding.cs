using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Onboarding.Commands;

public record CompleteOnboarding(
    string ClinicName,
    Guid SubscriptionPlanId,
    string BranchName,
    string AddressLine,
    int CountryGeoNameId,
    int StateGeoNameId,
    int CityGeoNameId,
    bool ProvideMedicalServices,
    Guid? SpecializationId
) : IRequest<Result>;

public class CompleteOnboardingHandler : IRequestHandler<CompleteOnboarding, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;

    public CompleteOnboardingHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<User> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<Result> Handle(CompleteOnboarding request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var existingClinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.OwnerUserId == userId, cancellationToken);
            
        if (existingClinic != null)
        {
            return Result.Failure(ErrorCodes.ALREADY_ONBOARDED, "User has already completed onboarding");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (user == null)
        {
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        if (!userRoles.Contains(UserRoles.ClinicOwner))
        {
            return Result.Failure(ErrorCodes.FORBIDDEN, "User must be clinic owner");
        }

        var subscriptionPlan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(sp => sp.Id == request.SubscriptionPlanId, cancellationToken);
            
        if (subscriptionPlan == null)
        {
            return Result.Failure(ErrorCodes.PLAN_NOT_FOUND, "The selected subscription plan does not exist");
        }

        var clinic = new Clinic
        {
            Name = request.ClinicName,
            OwnerUserId = userId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            OnboardingCompleted = true,
            IsActive = true
        };

        _context.Clinics.Add(clinic);

        var branch = new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = request.BranchName,
            AddressLine = request.AddressLine,
            CountryGeoNameId = request.CountryGeoNameId,
            StateGeoNameId = request.StateGeoNameId,
            CityGeoNameId = request.CityGeoNameId,
            IsMainBranch = true,
            IsActive = true
        };

        _context.ClinicBranches.Add(branch);

        // Create Staff and DoctorProfile records if owner will provide medical services
        if (request.ProvideMedicalServices)
        {
            var staff = new Domain.Entities.Staff
            {
                UserId = userId,
                ClinicId = clinic.Id,
                IsActive = true
            };

            _context.Staff.Add(staff);

            var doctorProfile = new DoctorProfile
            {
                StaffId = staff.Id,
                SpecializationId = request.SpecializationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.DoctorProfiles.Add(doctorProfile);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
