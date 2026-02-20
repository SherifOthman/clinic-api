using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Exceptions;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Password,
    string PhoneNumber
) : IRequest<Result>;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result>
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

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var registrationRequest = new UserRegistrationRequest(
                Email: request.Email,
                Password: request.Password,
                FirstName: request.FirstName,
                LastName: request.LastName,
                PhoneNumber: request.PhoneNumber,
                Role: Roles.ClinicOwner,
                ClinicId: null,
                UserName: request.UserName,
                EmailConfirmed: false,
                SendConfirmationEmail: true
            );

            await _userRegistrationService.RegisterUserAsync(registrationRequest, cancellationToken);
            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Registration failed: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {Email}", request.Email);
            return Result.Failure("REGISTRATION_FAILED", "An unexpected error occurred during registration");
        }
    }
}

