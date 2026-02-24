using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class UserRegistrationService : IUserRegistrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<UserRegistrationService> _logger;

    public UserRegistrationService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEmailTokenService emailTokenService,
        ILogger<UserRegistrationService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _emailTokenService = emailTokenService;
        _logger = logger;
    }

    public async Task<Result<Guid>> RegisterUserAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result.Failure<Guid>("EMAIL_ALREADY_EXISTS", "Email is already registered");
        }

        if (!string.IsNullOrEmpty(request.UserName) &&
            await _unitOfWork.Users.UsernameExistsAsync(request.UserName, cancellationToken))
        {
            return Result.Failure<Guid>("USERNAME_ALREADY_EXISTS", "Username is already taken");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName ?? request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsEmailConfirmed = request.EmailConfirmed,
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.Users.AddAsync(user, cancellationToken);

            var roles = await _unitOfWork.Users.GetRolesAsync(cancellationToken);
            var role = roles.FirstOrDefault(r => r.Name == request.Role);

            if (role == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<Guid>("INVALID_ROLE", $"Role '{request.Role}' not found");
            }

            await _unitOfWork.Users.AddUserRoleAsync(user.Id, role.Id, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error occurred while registering user: {Email}", request.Email);
            throw;
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
