using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Mapster;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class UserRegistrationService : IUserRegistrationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<UserRegistrationService> _logger;

    public UserRegistrationService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IUserManagementService userManagementService,
        IEmailConfirmationService emailConfirmationService,
        ILogger<UserRegistrationService> logger)
    {
        _context = context;
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
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Validate email and username uniqueness
            var validationResult = await ValidateUserUniquenessAsync(request, cancellationToken);
            if (validationResult.IsFailure)
                return validationResult;

            // Create user and assign role (these call SaveChanges internally via UserManager)
            var userIdResult = await CreateUserWithRoleAsync(request, cancellationToken);
            if (userIdResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return userIdResult;
            }

            var userId = userIdResult.Value;
            
            // Create type-specific entity (Doctor/Receptionist)
            await CreateTypeSpecificEntityAsync(userId, request.UserType, cancellationToken);
            
            // Save type-specific entities
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Commit transaction - all operations succeeded
            await transaction.CommitAsync(cancellationToken);

            // Send confirmation email (outside transaction - non-critical)
            await SendConfirmationEmailIfNeededAsync(request, userId, cancellationToken);

            _logger.LogInformation("User registered successfully: {Email} as {UserType}", 
                request.Email, request.UserType);

            return Result<Guid>.Ok(userId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            return Result<Guid>.Fail(MessageCodes.Exception.INTERNAL_ERROR);
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
            return Result<Guid>.FailField("email", MessageCodes.Validation.EMAIL_ALREADY_REGISTERED);
        }

        if (!string.IsNullOrEmpty(request.UserName))
        {
            existingUser = await _userManagementService.GetByUsernameAsync(request.UserName, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing username: {UserName}", request.UserName);
                return Result<Guid>.FailField("userName", MessageCodes.Validation.USERNAME_ALREADY_TAKEN);
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
            var errorCode = createResult.Code ?? MessageCodes.Authentication.USER_CREATION_FAILED;
            _logger.LogError("User creation failed for {Email}: {ErrorCode}", request.Email, errorCode);
            return Result<Guid>.Fail(errorCode);
        }

        _logger.LogInformation("User created successfully: {UserId}, assigning role...", userId);

        // Assign role based on UserType
        var roleName = GetRoleNameForUserType(request.UserType);
        _logger.LogInformation("Assigning role {RoleName} to user {UserId}", roleName, userId);
        
        var roleResult = await _userManagementService.AddToRoleAsync(user, roleName, cancellationToken);
        if (roleResult.IsFailure)
        {
            var errorCode = roleResult.Code ?? MessageCodes.Authentication.ROLE_ASSIGNMENT_FAILED;
            _logger.LogError("Role assignment failed for {Email}, Role: {RoleName}, Error: {ErrorCode}", 
                request.Email, roleName, errorCode);
            return Result<Guid>.Fail(errorCode);
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
