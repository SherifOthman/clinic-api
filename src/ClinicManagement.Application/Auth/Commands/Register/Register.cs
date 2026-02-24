using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
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

        var registrationResult = await _userRegistrationService.RegisterUserAsync(registrationRequest, cancellationToken);
        
        if (registrationResult.IsFailure)
        {
            _logger.LogWarning("Registration failed: {ErrorCode} - {Message}", registrationResult.ErrorCode, registrationResult.ErrorMessage);
            return Result.Failure(registrationResult.ErrorCode, registrationResult.ErrorMessage);
        }

        _logger.LogInformation("User registered successfully: {Email}", request.Email);

        return Result.Success();
    }
}

