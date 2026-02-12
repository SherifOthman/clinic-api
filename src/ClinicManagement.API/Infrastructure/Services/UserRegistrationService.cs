using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Extensions;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.API.Infrastructure.Services;

public class UserRegistrationService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly EmailConfirmationService _emailConfirmationService;
    private readonly ILogger<UserRegistrationService> _logger;

    public UserRegistrationService(
        ApplicationDbContext db,
        UserManager<User> userManager,
        EmailConfirmationService emailConfirmationService,
        ILogger<UserRegistrationService> logger)
    {
        _db = db;
        _userManager = userManager;
        _emailConfirmationService = emailConfirmationService;
        _logger = logger;
    }

    public async Task<Guid> RegisterUserAsync(
        UserRegistrationRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Use explicit transaction to ensure all operations succeed or fail together
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Validate email and username uniqueness
            await ValidateUserUniquenessAsync(request, cancellationToken);

            // Create user and assign role (these call SaveChanges internally via UserManager)
            var userId = await CreateUserWithRoleAsync(request, cancellationToken);
            
            // Create type-specific entity (Doctor/Receptionist)
            await CreateTypeSpecificEntityAsync(userId, request.UserType, cancellationToken);
            
            // Save type-specific entities
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction - all operations succeeded
            await transaction.CommitAsync(cancellationToken);

            // Send confirmation email (outside transaction - non-critical)
            await SendConfirmationEmailIfNeededAsync(request, userId, cancellationToken);

            _logger.LogInformation("User registered successfully: {Email} as {UserType}", 
                request.Email, request.UserType);

            return userId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task ValidateUserUniquenessAsync(
        UserRegistrationRequest request, 
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            throw new DomainException("EMAIL_ALREADY_EXISTS", "Email is already registered");
        }

        if (!string.IsNullOrEmpty(request.UserName))
        {
            existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing username: {UserName}", request.UserName);
                throw new DomainException("USERNAME_ALREADY_EXISTS", "Username is already taken");
            }
        }
    }

    private async Task<Guid> CreateUserWithRoleAsync(
        UserRegistrationRequest request, 
        CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid();

        // Create user account
        var user = new User
        {
            Id = userId,
            UserName = request.UserName ?? request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = request.EmailConfirmed,
            OnboardingCompleted = request.OnboardingCompleted
        };

        _logger.LogInformation("Creating user: {Email} with UserType: {UserType}", request.Email, request.UserType);

        var result = await _userManager.CreateAsync(user, request.Password);
        result.ThrowIfFailed();

        _logger.LogInformation("User created successfully: {UserId}, assigning role...", userId);

        // Assign role based on UserType
        var roleName = GetRoleNameForUserType(request.UserType);
        _logger.LogInformation("Assigning role {RoleName} to user {UserId}", roleName, userId);
        
        result = await _userManager.AddToRoleAsync(user, roleName);
        result.ThrowIfFailed();

        _logger.LogInformation("Role {RoleName} assigned successfully to user {UserId}", roleName, userId);

        return userId;
    }

    private async Task SendConfirmationEmailIfNeededAsync(
        UserRegistrationRequest request, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        if (request.SendConfirmationEmail && !request.EmailConfirmed)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                await _emailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);
            }
        }
    }

    private string GetRoleNameForUserType(UserType userType)
    {
        return userType switch
        {
            UserType.SuperAdmin => Roles.SuperAdmin,
            UserType.ClinicOwner => Roles.ClinicOwner,
            UserType.Doctor => Roles.Doctor,
            UserType.Receptionist => Roles.Receptionist,
            _ => throw new ArgumentException($"Unknown user type: {userType}")
        };
    }

    private async Task CreateTypeSpecificEntityAsync(Guid userId, UserType userType, CancellationToken cancellationToken)
    {
        switch (userType)
        {
            case UserType.Doctor:
                var doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                _db.Doctors.Add(doctor);
                break;

            case UserType.Receptionist:
                var receptionist = new Receptionist
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                _db.Receptionists.Add(receptionist);
                break;

            case UserType.ClinicOwner:
                // ClinicOwner doesn't need a separate entity
                break;

            case UserType.SuperAdmin:
                // SuperAdmin doesn't need a separate entity
                break;

            default:
                throw new ArgumentException($"Unknown user type: {userType}");
        }
    }
}

public record UserRegistrationRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserType UserType,
    string? UserName = null,
    bool EmailConfirmed = false,
    bool OnboardingCompleted = false,
    bool SendConfirmationEmail = true
);
