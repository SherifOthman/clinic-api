using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ISecurityAuditWriter _auditWriter;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUnitOfWork uow,
        UserManager<User> userManager,
        IEmailTokenService emailTokenService,
        ISecurityAuditWriter auditWriter,
        ILogger<RegisterHandler> logger)
    {
        _uow               = uow;
        _userManager       = userManager;
        _emailTokenService = emailTokenService;
        _auditWriter       = auditWriter;
        _logger            = logger;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Create Person first — the human being behind the account
        var person = new Person
        {
            FirstName      = request.FirstName,
            LastName       = request.LastName,
            Gender         = Enum.TryParse<Domain.Enums.Gender>(request.Gender, out var pg) ? pg : Domain.Enums.Gender.Male,
        };
        // Person is saved via User navigation — EF will insert it
        // We need to save it first to get the ID
        // Use a temporary approach: save person separately
        // (In a future cleanup, User.PersonId will be required)

        var user = new User
        {
            Email          = request.Email,
            UserName       = request.UserName ?? request.Email,
            FirstName      = request.FirstName,
            LastName       = request.LastName,
            PhoneNumber    = request.PhoneNumber,
            Gender         = Enum.TryParse<Domain.Enums.Gender>(request.Gender, out var rg) ? rg : Domain.Enums.Gender.Male,
            EmailConfirmed = false,
            Person         = person, // EF will create Person and set PersonId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user {Email}: {Errors}", request.Email, errors);
            return Result.Failure(ErrorCodes.USER_CREATION_FAILED, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.ClinicOwner);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to add user {Email} to role {Role}: {Errors}", request.Email, Roles.ClinicOwner, errors);
            await _userManager.DeleteAsync(user);
            return Result.Failure(ErrorCodes.ROLE_ASSIGNMENT_FAILED, $"Failed to assign role: {errors}");
        }

        try { await _emailTokenService.SendConfirmationEmailAsync(user, cancellationToken); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to send confirmation email to {Email}", request.Email); }

        await _auditWriter.WriteAsync(user.Id, user.FullName, user.UserName, user.Email,
            Roles.ClinicOwner, clinicId: null, "Register", cancellationToken: cancellationToken);

        _logger.LogInformation("User registered successfully: {Email} with role {Role}", request.Email, Roles.ClinicOwner);
        return Result.Success();
    }
}
