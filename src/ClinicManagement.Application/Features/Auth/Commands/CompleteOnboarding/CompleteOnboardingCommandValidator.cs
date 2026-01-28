using ClinicManagement.Application.Common.Services;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    private readonly IPhoneNumberValidationService _phoneValidationService;

    public CompleteOnboardingCommandValidator(IPhoneNumberValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;

        RuleFor(x => x.ClinicName)
            .NotEmpty()
            .WithMessage("Clinic name is required")
            .MaximumLength(100)
            .WithMessage("Clinic name must not exceed 100 characters")
            .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s'-]+$")
            .WithMessage("Clinic name can only contain letters, numbers, spaces, hyphens, and apostrophes");

        RuleFor(x => x.SubscriptionPlanId)
            .GreaterThan(0)
            .WithMessage("Please select a valid subscription plan");

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(100)
            .WithMessage("Branch name must not exceed 100 characters")
            .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s'-]+$")
            .WithMessage("Branch name can only contain letters, numbers, spaces, hyphens, and apostrophes");

        RuleFor(x => x.BranchAddress)
            .NotEmpty()
            .WithMessage("Branch address is required")
            .MaximumLength(200)
            .WithMessage("Branch address must not exceed 200 characters");

        RuleFor(x => x.CountryId)
            .GreaterThan(0)
            .WithMessage("Please select a valid country");

        RuleFor(x => x.CityId)
            .GreaterThan(0)
            .WithMessage("Please select a valid city");

        RuleFor(x => x.BranchPhoneNumbers)
            .NotEmpty()
            .WithMessage("At least one phone number is required");

        RuleForEach(x => x.BranchPhoneNumbers)
            .SetValidator(new BranchPhoneNumberValidator(_phoneValidationService));
    }
}

public class BranchPhoneNumberValidator : AbstractValidator<BranchPhoneNumberDto>
{
    private readonly IPhoneNumberValidationService _phoneValidationService;

    public BranchPhoneNumberValidator(IPhoneNumberValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .MaximumLength(25)
            .WithMessage("Phone number must not exceed 25 characters")
            .Must(phoneNumber => _phoneValidationService.IsValidPhoneNumber(phoneNumber))
            .WithMessage("Please enter a valid phone number");

        RuleFor(x => x.Label)
            .MaximumLength(50)
            .WithMessage("Label must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Label));
    }
}
