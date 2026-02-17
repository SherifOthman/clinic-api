using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Application.Common.Extensions;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class UserRegistrationService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly EmailConfirmationService _emailConfirmationService;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly ILogger<UserRegistrationService> _logger;

    public UserRegistrationService(
        ApplicationDbContext db,
        UserManager<User> userManager,
        EmailConfirmationService emailConfirmationService,
        DateTimeProvider dateTimeProvider,
        ILogger<UserRegistrationService> logger)
    {
        _db = db;
        _userManager = userManager;
        _emailConfirmationService = emailConfirmationService;
        _dateTimeProvider = dateTimeProvider;
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
            
            // Create Staff record for clinic membership
            await CreateStaffRecordAsync(userId, request.ClinicId, cancellationToken);
            
            // Create type-specific profile (only for doctors)
            await CreateTypeSpecificProfileAsync(userId, request.Role, cancellationToken);
            
            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction - all operations succeeded
            await transaction.CommitAsync(cancellationToken);

            // Send confirmation email (outside transaction - non-critical)
            await SendConfirmationEmailIfNeededAsync(request, userId, cancellationToken);

            _logger.LogInformation("User registered successfully: {Email} with role {Role}", 
                request.Email, request.Role);

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

        // Create user account (pure identity - no clinic data)
        var user = new User
        {
            Id = userId,
            UserName = request.UserName ?? request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = request.EmailConfirmed
        };

        _logger.LogInformation("Creating user: {Email} with role: {Role}", request.Email, request.Role);

        var result = await _userManager.CreateAsync(user, request.Password);
        result.ThrowIfFailed();

        _logger.LogInformation("User created successfully: {UserId}, assigning role...", userId);

        // Assign role
        _logger.LogInformation("Assigning role {Role} to user {UserId}", request.Role, userId);
        
        result = await _userManager.AddToRoleAsync(user, request.Role);
        result.ThrowIfFailed();

        _logger.LogInformation("Role {Role} assigned successfully to user {UserId}", request.Role, userId);

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

    private async Task CreateStaffRecordAsync(Guid userId, Guid? clinicId, CancellationToken cancellationToken)
    {
        // Only create Staff record if user belongs to a clinic
        // SuperAdmin doesn't have a clinic, so no Staff record
        if (clinicId.HasValue)
        {
            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ClinicId = clinicId.Value,
                IsActive = true,
                HireDate = _dateTimeProvider.UtcNow
            };

            _db.Staff.Add(staff);
            _logger.LogInformation("Created Staff record for user {UserId} in clinic {ClinicId}", userId, clinicId);
        }
    }

    private async Task CreateTypeSpecificProfileAsync(Guid userId, string role, CancellationToken cancellationToken)
    {
        // Only doctors need a profile with additional data
        if (role == Roles.Doctor)
        {
            // Get the staff record we just created
            var staff = await _db.Staff.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
            if (staff != null)
            {
                var doctorProfile = new DoctorProfile
                {
                    Id = Guid.NewGuid(),
                    StaffId = staff.Id
                };
                _db.DoctorProfiles.Add(doctorProfile);
                _logger.LogInformation("Created DoctorProfile for staff {StaffId}", staff.Id);
            }
        }
        
        // Receptionist and ClinicOwner don't need profiles - just Staff record + Role
    }
}

public record UserRegistrationRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role, // ASP.NET Identity role name (e.g., "ClinicOwner", "Doctor")
    Guid? ClinicId = null, // Clinic membership (null for SuperAdmin)
    string? UserName = null,
    bool EmailConfirmed = false,
    bool SendConfirmationEmail = true
);
