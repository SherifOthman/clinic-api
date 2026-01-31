using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class ClinicManagementService : IClinicManagementService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ClinicManagementService> _logger;

    public ClinicManagementService(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ILogger<ClinicManagementService> logger)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<bool>> AssignUserToClinicAsync(int userId, int clinicId, bool isOwner = false, CancellationToken cancellationToken = default)
    {
        // Check if user is already assigned to this clinic
        var existingAssignment = await _context.UserClinics
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClinicId == clinicId, cancellationToken);

        if (existingAssignment != null)
        {
            // Update existing assignment
            existingAssignment.IsOwner = isOwner;
            existingAssignment.IsActive = true;
            _logger.LogInformation("Updated user {UserId} assignment to clinic {ClinicId}, IsOwner: {IsOwner}", userId, clinicId, isOwner);
        }
        else
        {
            // Create new assignment
            var userClinic = new UserClinic
            {
                UserId = userId,
                ClinicId = clinicId,
                IsOwner = isOwner,
                IsActive = true,
                JoinedAt = _dateTimeProvider.UtcNow
            };

            _context.UserClinics.Add(userClinic);
            _logger.LogInformation("Assigned user {UserId} to clinic {ClinicId}, IsOwner: {IsOwner}", userId, clinicId, isOwner);
        }

        // If this is the user's first clinic or they're becoming an owner, set it as current clinic
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user != null && (user.CurrentClinicId == null || isOwner))
        {
            user.CurrentClinicId = clinicId;
            if (user.ClinicId == null) // Backward compatibility
            {
                user.ClinicId = clinicId;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RemoveUserFromClinicAsync(int userId, int clinicId, CancellationToken cancellationToken = default)
    {
        var userClinic = await _context.UserClinics
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClinicId == clinicId, cancellationToken);

        if (userClinic == null)
        {
            return Result<bool>.Fail(MessageCodes.Business.ENTITY_NOT_FOUND);
        }

        _context.UserClinics.Remove(userClinic);

        // If this was the user's current clinic, switch to another clinic or clear it
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user != null && user.CurrentClinicId == clinicId)
        {
            var otherClinic = await _context.UserClinics
                .Where(uc => uc.UserId == userId && uc.ClinicId != clinicId && uc.IsActive)
                .OrderByDescending(uc => uc.IsOwner)
                .FirstOrDefaultAsync(cancellationToken);

            user.CurrentClinicId = otherClinic?.ClinicId;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed user {UserId} from clinic {ClinicId}", userId, clinicId);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<IEnumerable<Clinic>>> GetUserClinicsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var clinics = await _context.UserClinics
            .Include(uc => uc.Clinic)
            .ThenInclude(c => c.SubscriptionPlan)
            .Where(uc => uc.UserId == userId && uc.IsActive)
            .Select(uc => uc.Clinic)
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<Clinic>>.Ok(clinics);
    }

    public async Task<Result<bool>> CanUserAccessClinicAsync(int userId, int clinicId, CancellationToken cancellationToken = default)
    {
        var hasAccess = await _context.UserClinics
            .AnyAsync(uc => uc.UserId == userId && uc.ClinicId == clinicId && uc.IsActive, cancellationToken);

        return Result<bool>.Ok(hasAccess);
    }

    public async Task<Result<bool>> IsUserClinicOwnerAsync(int userId, int clinicId, CancellationToken cancellationToken = default)
    {
        var isOwner = await _context.UserClinics
            .AnyAsync(uc => uc.UserId == userId && uc.ClinicId == clinicId && uc.IsOwner && uc.IsActive, cancellationToken);

        return Result<bool>.Ok(isOwner);
    }

    public async Task<Result<int>> GetUserClinicCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var count = await _context.UserClinics
            .Where(uc => uc.UserId == userId && uc.IsOwner && uc.IsActive)
            .CountAsync(cancellationToken);

        return Result<int>.Ok(count);
    }

    public async Task<Result<bool>> CanUserCreateMoreClinicsAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Get user's current clinic and its subscription plan
        var user = await _context.Users
            .Include(u => u.CurrentClinic)
            .ThenInclude(c => c!.SubscriptionPlan)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user?.CurrentClinic?.SubscriptionPlan == null)
        {
            return Result<bool>.Ok(false);
        }

        var subscriptionPlan = user.CurrentClinic.SubscriptionPlan;
        
        // If unlimited clinics (-1), user can create more
        if (subscriptionPlan.MaxClinics == -1)
        {
            return Result<bool>.Ok(true);
        }

        // Check current owned clinic count
        var ownedClinicCount = await _context.UserClinics
            .Where(uc => uc.UserId == userId && uc.IsOwner && uc.IsActive)
            .CountAsync(cancellationToken);

        var canCreateMore = ownedClinicCount < subscriptionPlan.MaxClinics;
        return Result<bool>.Ok(canCreateMore);
    }
}