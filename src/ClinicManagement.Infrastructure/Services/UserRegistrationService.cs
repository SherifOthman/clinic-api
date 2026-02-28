using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class UserRegistrationService : IUserRegistrationService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<UserRegistrationService> _logger;

    public UserRegistrationService(
        IApplicationDbContext context,
        UserManager<User> userManager,
        IPasswordHasher passwordHasher,
        IEmailTokenService emailTokenService,
        ILogger<UserRegistrationService> logger)
    {
        _context = context;
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _emailTokenService = emailTokenService;
        _logger = logger;
    }

    public async Task<Result<Guid>> RegisterUserAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        // Check if email exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            return Result.Failure<Guid>("EMAIL_ALREADY_EXISTS", "Email is already registered");
        }

        // Check if username exists
        if (!string.IsNullOrEmpty(request.UserName) &&
            await _context.Users.AnyAsync(u => u.UserName == request.UserName, cancellationToken))
        {
            return Result.Failure<Guid>("USERNAME_ALREADY_EXISTS", "Username is already taken");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName ?? request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = request.EmailConfirmed,
        };

        // Use UserManager to create user with password
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user {Email}: {Errors}", request.Email, errors);
            return Result.Failure<Guid>("USER_CREATION_FAILED", errors);
        }

        // Add user to role using UserManager
        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to add user {Email} to role {Role}: {Errors}", request.Email, request.Role, errors);
            
            // Rollback: delete the user
            await _userManager.DeleteAsync(user);
            return Result.Failure<Guid>("ROLE_ASSIGNMENT_FAILED", $"Failed to assign role: {errors}");
        }

        if (request.SendConfirmationEmail && !request.EmailConfirmed)
        {
            try
            {
                await _emailTokenService.SendConfirmationEmailAsync(user, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", request.Email);
            }
        }

        _logger.LogInformation("User registered successfully: {Email} with role {Role}", request.Email, request.Role);
        return Result.Success(user.Id);
    }
}
