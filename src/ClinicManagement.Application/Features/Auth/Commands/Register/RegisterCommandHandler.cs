using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IUserRegistrationService _userRegistrationService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRegistrationService userRegistrationService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRegistrationService = userRegistrationService;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registrationRequest = request.Adapt<UserRegistrationRequest>();
        registrationRequest.UserType = UserType.ClinicOwner;
        registrationRequest.EmailConfirmed = false;
        registrationRequest.OnboardingCompleted = false;
        registrationRequest.SendConfirmationEmail = true;

        var result = await _userRegistrationService.RegisterUserAsync(registrationRequest, cancellationToken);
        
        if (result.IsFailure)
        {
            if (result.Errors != null && result.Errors.Any())
                return Result.Fail(result.Errors);
            
            return Result.Fail(result.Code ?? MessageCodes.Authentication.REGISTRATION_FAILED);
        }

        _logger.LogInformation("Clinic owner registered successfully: {Email}", request.Email);
        
        return Result.Ok();
    }
}