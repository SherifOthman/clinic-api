using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Staff.Commands.InviteStaff;

public class InviteStaffCommandHandler : IRequestHandler<InviteStaffCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InviteStaffCommandHandler> _logger;

    public InviteStaffCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IEmailSender emailSender,
        ICurrentUserService currentUserService,
        ILogger<InviteStaffCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _emailSender = emailSender;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        // Get ClinicId and UserId from authenticated user context (secure - not from client)
        var clinicId = _currentUserService.GetClinicId();
        if (!clinicId.HasValue)
        {
            _logger.LogWarning("Staff invitation failed: User not associated with any clinic");
            return Result.Fail("User is not associated with any clinic");
        }

        var invitedByUserId = _currentUserService.GetUserId();
        
        _logger.LogInformation("Processing staff invitation for {Email} as {Role} by user {UserId} in clinic {ClinicId}", 
            request.Email, request.Role, invitedByUserId, clinicId.Value);

        try
        {
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
            var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId.Value, cancellationToken);
            if (clinic == null)
            {
                return Result.Fail("Clinic not found");
            }

            // 4. Verify specialization for doctors
            if (request.Role == UserRole.Doctor && request.SpecializationId.HasValue)
            {
                var specialization = await _unitOfWork.Specializations.GetByIdAsync(
                    request.SpecializationId.Value, cancellationToken);
                
                if (specialization == null)
                {
                    return Result.FailField("specializationId", "Invalid specialization");
                }
            }

            // 5. Create invitation token
            var invitationToken = Guid.NewGuid().ToString("N");
            var invitationExpiry = DateTime.UtcNow.AddDays(7); // 7 days to accept

            // 6. Create pending user record
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email, // Will be changed during registration
                PhoneNumber = request.PhoneNumber,
                ClinicId = clinicId.Value,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            // Generate a temporary password (user will set their own during registration)
            var tempPassword = Guid.NewGuid().ToString();
            var createResult = await _identityService.CreateUserAsync(user, tempPassword, cancellationToken);
            
            if (!createResult.IsSuccess)
            {
                _logger.LogError("Failed to create user: {Errors}", 
                    string.Join(", ", createResult.Errors!.Select(e => e.Message)));
                return Result.Fail(createResult.Errors!);
            }

            // 7. Assign role
            await _identityService.SetUserRoleAsync(user, request.Role.ToString(), cancellationToken);

            // 8. Create Doctor or Receptionist record
            if (request.Role == UserRole.Doctor)
            {
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    ClinicId = clinicId.Value,
                    SpecializationId = request.SpecializationId!.Value,
                    IsActive = false // Will be activated when they complete registration
                };
                _unitOfWork.Doctors.Add(doctor);
            }
            else if (request.Role == UserRole.Receptionist)
            {
                var receptionist = new Receptionist
                {
                    UserId = user.Id,
                    ClinicId = clinicId.Value,
                    IsActive = false
                };
                _unitOfWork.Receptionists.Add(receptionist);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 9. Send invitation email
            var invitationLink = $"{GetBaseUrl()}/accept-invitation?token={invitationToken}&userId={user.Id}";
            
            await _emailSender.SendEmailAsync(
                request.Email,
                $"Invitation to join {clinic.Name}",
                GetInvitationEmailTemplate(
                    request.FirstName,
                    clinic.Name,
                    inviter.FirstName + " " + inviter.LastName,
                    request.Role.ToString(),
                    invitationLink,
                    invitationExpiry
                ),
                cancellationToken
            );

            _logger.LogInformation("Staff invitation sent successfully to {Email}", request.Email);

            return Result.Ok($"Invitation sent successfully to {request.Email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending staff invitation to {Email}", request.Email);
            return Result.Fail("An error occurred while sending the invitation. Please try again.");
        }
    }

    private string GetBaseUrl()
    {
        // This should come from configuration
        return "https://yourapp.com";
    }

    private string GetInvitationEmailTemplate(
        string firstName,
        string clinicName,
        string inviterName,
        string role,
        string invitationLink,
        DateTime expiryDate)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>You've been invited to join {clinicName}!</h2>
                <p>Hi {firstName},</p>
                <p>{inviterName} has invited you to join <strong>{clinicName}</strong> as a <strong>{role}</strong>.</p>
                <p>To accept this invitation and complete your registration, please click the button below:</p>
                <p style='margin: 30px 0;'>
                    <a href='{invitationLink}' 
                       style='background-color: #4CAF50; color: white; padding: 14px 28px; 
                              text-decoration: none; border-radius: 4px; display: inline-block;'>
                        Accept Invitation
                    </a>
                </p>
                <p>Or copy and paste this link into your browser:</p>
                <p style='color: #666; word-break: break-all;'>{invitationLink}</p>
                <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                    This invitation will expire on {expiryDate:MMMM dd, yyyy} at {expiryDate:HH:mm} UTC.
                </p>
                <p style='color: #999; font-size: 12px;'>
                    If you didn't expect this invitation, you can safely ignore this email.
                </p>
            </body>
            </html>
        ";
    }
}
