using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IEmailSender emailSender)
    {
        _identityService = identityService;
        _emailSender = emailSender;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExist = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userExist != null)
        {
            return Result.FailField("email", "This email address is already registered.");
        }

        userExist = await _identityService.GetByUsernameAsync(request.Username, cancellationToken);
        if (userExist != null)
        {
            return Result.FailField("username", "This username is already taken.");
        }

        var user = request.Adapt<User>();

        var result = await _identityService.CreateUserAsync(user, request.Password, cancellationToken);
        if (!result.Success)
        {
            return result;
        }

        await _identityService.SetUserRoleAsync(user, request.Role.ToString(), cancellationToken);
        
        // Send confirmation email - let GlobalExceptionMiddleware handle any failures
        await _identityService.SendConfirmationEmailAsync(user, cancellationToken);
        return Result.Ok("Registration successful! Please check your email to verify your account.");
    }
}
