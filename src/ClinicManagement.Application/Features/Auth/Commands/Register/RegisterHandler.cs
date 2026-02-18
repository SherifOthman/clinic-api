using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRegistrationService _userRegistrationService;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUserRegistrationService userRegistrationService,
        ILogger<RegisterHandler> logger)
    {
        _userRegistrationService = userRegistrationService;
        _logger = logger;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // SECURITY: Public registration can only create ClinicOwner accounts
            // SuperAdmin accounts must be created through seeding
            // Staff accounts (Doctor, Receptionist) are created by ClinicOwner through admin panel
            var registrationRequest = new UserRegistrationRequest(
                Email: request.Email,
                Password: request.Password,
                FirstName: request.FirstName,
                LastName: request.LastName,
                PhoneNumber: request.PhoneNumber,
                Role: Roles.ClinicOwner, // Public registration = ClinicOwner only
                ClinicId: null, // No clinic yet - will be created during onboarding
                UserName: request.UserName,
                EmailConfirmed: false,
                SendConfirmationEmail: true
            );

            var userId = await _userRegistrationService.RegisterUserAsync(registrationRequest, cancellationToken);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return new RegisterResult(
                Success: true,
                UserId: userId,
                ErrorCode: null,
                ErrorMessage: null
            );
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Registration failed: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return new RegisterResult(
                Success: false,
                UserId: null,
                ErrorCode: ex.ErrorCode,
                ErrorMessage: ex.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {Email}", request.Email);
            return new RegisterResult(
                Success: false,
                UserId: null,
                ErrorCode: "REGISTRATION_FAILED",
                ErrorMessage: "An unexpected error occurred during registration"
            );
        }
    }
}
