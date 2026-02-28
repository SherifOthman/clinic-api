using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Commands;

public record AcceptInvitationWithRegistrationCommand(
    string Token,
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string PhoneNumber
) : IRequest<Result>;

public class AcceptInvitationWithRegistrationHandler : IRequestHandler<AcceptInvitationWithRegistrationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public AcceptInvitationWithRegistrationHandler(
        IApplicationDbContext context,
        UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result> Handle(AcceptInvitationWithRegistrationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _context.StaffInvitations
            .FirstOrDefaultAsync(si => si.InvitationToken == request.Token, cancellationToken);

        if (invitation == null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        var now = DateTime.UtcNow;

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            Email = invitation.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result.Failure(ErrorCodes.USER_CREATION_FAILED, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, invitation.Role);
        
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return Result.Failure(ErrorCodes.ROLE_ASSIGNMENT_FAILED, errors);
        }

        var acceptResult = invitation.Accept(user.Id, now);
        if (acceptResult.IsFailure)
        {
            await _userManager.DeleteAsync(user);
            return acceptResult;
        }

        var staff = new Domain.Entities.Staff
        {
            UserId = user.Id,
            ClinicId = invitation.ClinicId,
            IsActive = true,
            HireDate = now
        };

        _context.Staff.Add(staff);

        if (invitation.Role == "Doctor")
        {
            var doctorProfile = new DoctorProfile
            {
                StaffId = staff.Id,
                SpecializationId = invitation.SpecializationId
            };

            _context.DoctorProfiles.Add(doctorProfile);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
