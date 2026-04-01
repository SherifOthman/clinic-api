using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IApplicationDbContext context,
        UserManager<User> userManager,
        IEmailTokenService emailTokenService,
        ILogger<RegisterHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _emailTokenService = emailTokenService;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            return Result.Failure(ErrorCodes.EMAIL_ALREADY_EXISTS, "Email is already registered");
        }

        if (!string.IsNullOrEmpty(request.UserName) &&
            await _context.Users.AnyAsync(u => u.UserName == request.UserName, cancellationToken))
        {
            return Result.Failure(ErrorCodes.USERNAME_ALREADY_EXISTS, "Username is already taken");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName ?? request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsMale =  request.Gender == "male",
            EmailConfirmed = false
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

        try
        {
            await _emailTokenService.SendConfirmationEmailAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", request.Email);
        }

        _logger.LogInformation("User registered successfully: {Email} with role {Role}", request.Email, Roles.ClinicOwner);

        return Result.Success();
    }
}
