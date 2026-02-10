using ClinicManagement.Application.Common.Interfaces;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    private readonly IPhoneValidationService _phoneValidationService;

    public UpdateProfileCommandValidator(IPhoneValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhoneNumber).WithMessage("Phone number format is invalid")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(500)
            .WithMessage("Profile image URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl));
    }

    private bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return true;
        
        var result = _phoneValidationService.ValidatePhoneNumber(phoneNumber);
        return result.IsValid;
    }
}