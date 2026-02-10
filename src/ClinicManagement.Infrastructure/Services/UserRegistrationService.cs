using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class UserRegistrationService : IUserRegistrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<UserRegistrationService> _logger;

    public UserRegistrationService(
        IUnitOfWork unitOfWork,
        IUserManagementService userManagementService,
        IEmailConfirmationService emailConfirmationService,
        ILogger<UserRegistrationService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManagementService = userManagementService;
        _emailConfirmationService = emailConfirmationService;
        _logger = logger;
    }

    public async Task<Result<Guid>> RegisterUserAsync(
        UserRegistrationRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Use explicit transaction to ensure all operations succeed or fail together
        // This is needed because UserManager.CreateAsync and AddToRoleAsync auto-commit
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Validate email and username uniqueness
            var validationResult = await ValidateUserUniquenessAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return validationResult;
            }

            // Create user and assign role (these call SaveChanges internally via UserManager)
            var userIdResult = await CreateUserWithRoleAsync(request, cancellationToken);
            if (userIdResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return userIdResult;
            }

            var userId = userIdResult.Value;
            
            // Create type-specific entity (Doctor/Receptionist)
            await CreateTypeSpecificEntityAsync(userId, request.UserType, cancellationToken);
            
            // Save type-specific entities
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Commit transaction - all operations succeeded
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Send confirmation email (outside transaction - non-critical)
            await SendConfirmationEmailIfNeededAsync(request, userId, cancellationToken);

            _logger.LogInformation("User registered successfully: {Email} as {UserType}", 
                request.Email, request.UserType);

            return Result<Guid>.Ok(userId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            return Result<Guid>.FailSystem("INTERNAL_ERROR", "An error occurred during registration");
        }
    }

    private async Task<Result<Guid>> ValidateUserUniquenessAsync(
        UserRegistrationRequest request, 
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return Result<Guid>.FailValidation("email", "Email is already registered");
        }

        if (!string.IsNullOrEmpty(request.UserName))
        {
            existingUser = await _userManagementService.GetByUsernameAsync(request.UserName, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing username: {UserName}", request.UserName);
                return Result<Guid>.FailValidation("userName", "Username is already taken");
            }
        }

        return Result<Guid>.Ok(Guid.Empty); // Success, value not used
    }

    private async Task<Result<Guid>> CreateUserWithRoleAsync(
        UserRegistrationRequest request, 
        CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid();

        // Create user account using Mapster
        var user = request.Adapt<User>();
        user.Id = userId;
        user.UserName = request.UserName ?? request.Email;

        _logger.LogInformation("Creating user: {Email} with UserType: {UserType}", request.Email, request.UserType);

        var createResult = await _userManagementService.CreateUserAsync(user, request.Password, cancellationToken);
        if (createResult.IsFailure)
        {
            _logger.LogError("User creation failed for {Email}: {Message}", request.Email, createResult.Message);
            return Result<Guid>.FailSystem("USER_CREATION_FAILED", createResult.Message ?? "Failed to create user account");
        }

        _logger.LogInformation("User created successfully: {UserId}, assigning role...", userId);

        // Assign role based on UserType
        var roleName = GetRoleNameForUserType(request.UserType);
        _logger.LogInformation("Assigning role {RoleName} to user {UserId}", roleName, userId);
        
        var roleResult = await _userManagementService.AddToRoleAsync(user, roleName, cancellationToken);
        if (roleResult.IsFailure)
        {
            _logger.LogError("Role assignment failed for {Email}, Role: {RoleName}, Error: {Message}", 
                request.Email, roleName, roleResult.Message);
            return Result<Guid>.FailSystem("ROLE_ASSIGNMENT_FAILED", roleResult.Message ?? "Failed to assign user role");
        }

        _logger.LogInformation("Role {RoleName} assigned successfully to user {UserId}", roleName, userId);

        return Result<Guid>.Ok(userId);
    }

    private async Task SendConfirmationEmailIfNeededAsync(
        UserRegistrationRequest request, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        if (request.SendConfirmationEmail && !request.EmailConfirmed)
        {
            var user = await _userManagementService.GetUserByIdAsync(userId, cancellationToken);
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
                await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
                break;

            case UserType.Receptionist:
                var receptionist = new Receptionist
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                await _unitOfWork.Repository<Receptionist>().AddAsync(receptionist);
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
