using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Commands;

public record SetOwnerAsDoctor(
    Guid? SpecializationId,
    string? LicenseNumber,
    int? YearsOfExperience
) : IRequest<Result>;

public class SetOwnerAsDoctorHandler : IRequestHandler<SetOwnerAsDoctor, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;

    public SetOwnerAsDoctorHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<User> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<Result> Handle(SetOwnerAsDoctor request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (user == null)
        {
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        if (!userRoles.Contains(UserRoles.ClinicOwner))
        {
            return Result.Failure(ErrorCodes.FORBIDDEN, "Only clinic owners can use this endpoint");
        }

        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.OwnerUserId == userId, cancellationToken);
            
        if (clinic == null)
        {
            return Result.Failure(ErrorCodes.CLINIC_NOT_FOUND, "Clinic not found. Please complete onboarding first.");
        }

        // Check if user already has a staff record
        var existingStaff = await _context.Staff
            .Include(s => s.DoctorProfile)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ClinicId == clinic.Id, cancellationToken);

        if (existingStaff != null && existingStaff.DoctorProfile != null)
        {
            return Result.Failure(ErrorCodes.ALREADY_EXISTS, "You are already registered as a doctor");
        }

        // Add Doctor role if not already present
        if (!userRoles.Contains(UserRoles.Doctor))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Doctor);
            if (!roleResult.Succeeded)
            {
                return Result.Failure(ErrorCodes.OPERATION_FAILED, "Failed to assign doctor role");
            }
        }

        // Create staff record if it doesn't exist
        if (existingStaff == null)
        {
            existingStaff = new Domain.Entities.Staff
            {
                UserId = userId,
                ClinicId = clinic.Id,
                IsActive = true
            };

            _context.Staff.Add(existingStaff);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Create doctor profile
        var doctorProfile = new DoctorProfile
        {
            StaffId = existingStaff.Id,
            SpecializationId = request.SpecializationId,
            LicenseNumber = request.LicenseNumber,
            YearsOfExperience = request.YearsOfExperience,
            CreatedAt = DateTime.UtcNow
        };

        _context.DoctorProfiles.Add(doctorProfile);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
