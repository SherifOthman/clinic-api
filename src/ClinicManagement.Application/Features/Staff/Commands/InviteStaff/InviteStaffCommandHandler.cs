using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Templates;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Application.Features.Staff.Commands.InviteStaff;

public class InviteStaffCommandHandler : IRequestHandler<InviteStaffCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly ICurrentUserService _currentUserService;
    private readonly SmtpOptions _smtpOptions;

    public InviteStaffCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IEmailSender emailSender,
        ICurrentUserService currentUserService,
        IOptions<SmtpOptions> smtpOptions)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _emailSender = emailSender;
        _currentUserService = currentUserService;
        _smtpOptions = smtpOptions.Value;
    }

    public async Task<Result> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        // Get ClinicId and UserId from authenticated user context
        var clinicId = _currentUserService.GetRequiredClinicId();
        var invitedByUserId = _currentUserService.GetRequiredUserId();

        // 1. Verify inviter exists
        var inviter = await _unitOfWork.Users.GetByIdAsync(invitedByUserId, cancellationToken);
        if (inviter == null)
        {
            return Result.Fail("Invalid inviter");
        }

        // 2. Check if user already exists
        var existingUser = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return Result.FailField("email", "A user with this email already exists");
        }

        // 3. Verify clinic exists
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId, cancellationToken);
        if (clinic == null)
        {
            return Result.Fail("Clinic not found");
        }

        // 4. Create invitation token
        var invitationToken = Guid.NewGuid().ToString("N");
        var invitationExpiry = DateTime.UtcNow.AddDays(7); // 7 days to accept

        // 5. Create pending user record using Mapster
        var user = request.Adapt<User>();

        // Generate a temporary password (user will set their own during registration)
        var tempPassword = Guid.NewGuid().ToString();
        var createResult = await _identityService.CreateUserAsync(user, tempPassword, cancellationToken);
        
        if (!createResult.Success)
        {
            return createResult;
        }

        // 6. Assign role
        var roleResult = await _identityService.SetUserRoleAsync(user, request.Role.ToString(), cancellationToken);
        if (!roleResult.Success)
        {
            return roleResult;
        }

        // 7. Create Doctor or Receptionist record
        if (request.Role == UserRole.Doctor)
        {
            var doctor = new Doctor
            {
                UserId = user.Id,
                ClinicId = clinicId,
                IsActive = false // Will be activated when they complete registration
            };
            _unitOfWork.Doctors.Add(doctor);
        }
        else if (request.Role == UserRole.Receptionist)
        {
            var receptionist = new Receptionist
            {
                UserId = user.Id,
                ClinicId = clinicId,
                IsActive = false
            };
            _unitOfWork.Receptionists.Add(receptionist);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Send invitation email
        var invitationLink = $"{_smtpOptions.FrontendUrl}/accept-invitation?token={invitationToken}&userId={user.Id}";
        
        await _emailSender.SendEmailAsync(
            request.Email,
            $"Invitation to join {clinic.Name}",
            EmailTemplates.StaffInvitation(
                request.FirstName,
                clinic.Name,
                inviter.FirstName + " " + inviter.LastName,
                request.Role.ToString(),
                invitationLink,
                invitationExpiry
            ),
            cancellationToken
        );

        return Result.Ok($"Invitation sent successfully to {request.Email}");
    }
}
