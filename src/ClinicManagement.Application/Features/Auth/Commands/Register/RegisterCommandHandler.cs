using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserManagementService userManagementService, 
        IEmailConfirmationService emailConfirmationService,
        IPhoneNumberValidationService phoneNumberValidationService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _emailConfirmationService = emailConfirmationService;
        _phoneNumberValidationService = phoneNumberValidationService;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExist = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userExist != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return Result.FailField("email", ApplicationErrors.Validation.EMAIL_ALREADY_REGISTERED);
        }

        userExist = await _userManagementService.GetByUsernameAsync(request.Username, cancellationToken);
        if (userExist != null)
        {
            _logger.LogWarning("Registration attempt with existing username: {Username}", request.Username);
            return Result.FailField("username", ApplicationErrors.Validation.USERNAME_ALREADY_TAKEN);
        }

        var user = request.Adapt<User>();
        
        // Format phone number to E.164 format for consistent storage
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            user.PhoneNumber = _phoneNumberValidationService.GetE164Format(request.PhoneNumber);
        }
        
        var result = await _userManagementService.CreateUserAsync(user, request.Password, cancellationToken);
        if (!result.Success)
            return result;

        await _emailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);
        
        _logger.LogInformation("User registered successfully: {Email}", request.Email);
        
        return Result.Ok();
    }
}
