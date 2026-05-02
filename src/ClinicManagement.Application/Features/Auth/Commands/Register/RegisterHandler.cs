using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;
    private readonly IEmailTokenService _emailTokenService;
    private readonly IAuditWriter _audit;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUnitOfWork uow,
        UserManager<User> userManager,
        IEmailTokenService emailTokenService,
        IAuditWriter audit,
        ILogger<RegisterHandler> logger)
    {
        _uow               = uow;
        _userManager       = userManager;
        _emailTokenService = emailTokenService;
        _audit             = audit;
        _logger            = logger;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email          = request.Email,
            UserName       = request.UserName ?? request.Email,
            PhoneNumber    = request.PhoneNumber,
            EmailConfirmed = false,
            FullName       = request.FullName,
            Gender         = Enum.TryParse<Gender>(request.Gender, out var pg) ? pg : Gender.Male,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user {Email}: {Errors}", request.Email, errors);
            return Result.Failure(ErrorCodes.USER_CREATION_FAILED, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.ClinicOwner);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to add user {Email} to role {Role}: {Errors}", request.Email, UserRoles.ClinicOwner, errors);
            await _userManager.DeleteAsync(user);
            return Result.Failure(ErrorCodes.ROLE_ASSIGNMENT_FAILED, $"Failed to assign role: {errors}");
        }

        try { await _emailTokenService.SendConfirmationEmailAsync(user, cancellationToken); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP ERROR: Failed to send confirmation email to {Email}. Error: {Message}",
                request.Email, ex.Message);
        }

        await _audit.WriteEventAsync("Register",
            overrideUserId: user.Id, overrideFullName: user.FullName,
            overrideEmail: user.Email, overrideRole: UserRoles.ClinicOwner, ct: cancellationToken);

        _logger.LogInformation("User registered successfully: {Email} with role {Role}", request.Email, UserRoles.ClinicOwner);
        return Result.Success();
    }
}
