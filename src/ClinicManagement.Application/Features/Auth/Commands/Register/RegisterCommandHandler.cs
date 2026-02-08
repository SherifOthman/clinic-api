using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserManagementService userManagementService, 
        IEmailConfirmationService emailConfirmationService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _emailConfirmationService = emailConfirmationService;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExist = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userExist != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return Result.FailField("email", MessageCodes.Validation.EMAIL_ALREADY_REGISTERED);
        }

        userExist = await _userManagementService.GetByUsernameAsync(request.UserName, cancellationToken);
        if (userExist != null)
        {
            _logger.LogWarning("Registration attempt with existing username: {UserName}", request.UserName);
            return Result.FailField("username", MessageCodes.Validation.USERNAME_ALREADY_TAKEN);
        }

        var user = request.Adapt<User>();   
        
        var result = await _userManagementService.CreateUserAsync(user, request.Password, cancellationToken);
        if (!result.Success)
            return result;

        await _emailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);
        
        _logger.LogInformation("User registered successfully: {Email}", request.Email);
        
        return Result.Ok();
    }
}
