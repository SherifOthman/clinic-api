using ClinicManagement.Application.Common.Interfaces;
using FluentValidation;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    private readonly IPhoneValidationService _phoneValidationService;

    public CompleteOnboardingCommandValidator(IPhoneValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;

        RuleFor(x => x.Dto.ClinicName)
            .NotEmpty().WithMessage("Clinic name is required")
            .MaximumLength(200).WithMessage("Clinic name cannot exceed 200 characters");

        RuleFor(x => x.Dto.BranchName)
            .NotEmpty().WithMessage("Branch name is required")
            .MaximumLength(200).WithMessage("Branch name cannot exceed 200 characters");

        RuleFor(x => x.Dto.BranchAddress)
            .NotEmpty().WithMessage("Branch address is required")
            .MaximumLength(500).WithMessage("Branch address cannot exceed 500 characters");

        RuleFor(x => x.Dto.BranchPhoneNumbers)
            .NotEmpty().WithMessage("At least one phone number is required")
            .Must(phones => phones != null && phones.Count > 0)
            .WithMessage("At least one phone number is required");

        RuleForEach(x => x.Dto.BranchPhoneNumbers)
            .ChildRules(phone =>
            {
                phone.RuleFor(p => p.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required")
                    .Must(BeValidPhoneNumber).WithMessage("Phone number format is invalid");
            });
    }

    private bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return false;
        
        var result = _phoneValidationService.ValidatePhoneNumber(phoneNumber);
        return result.IsValid;
    }
}