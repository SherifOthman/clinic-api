using ClinicManagement.Application.Abstractions.Authentication;`nusing ClinicManagement.Application.Abstractions.Email;`nusing ClinicManagement.Application.Abstractions.Services;`nusing ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<Result>;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(50)
            .WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(15)
            .WithMessage("Phone number must not exceed 15 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<UpdateProfileHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUser.UserId!.Value, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", _currentUser.UserId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Profile updated successfully for user: {UserId}", user.Id);

        return Result.Success();
    }
}
