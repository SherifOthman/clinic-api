using ClinicManagement.Application.Common.Constants;
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
            .WithMessage(MessageCodes.Fields.CLINIC_NAME_REQUIRED)
            .MaximumLength(100)
            .WithMessage(MessageCodes.Fields.CLINIC_NAME_MAX_LENGTH)
            .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s'.-]+$")
            .WithMessage(MessageCodes.Fields.CLINIC_NAME_INVALID_CHARACTERS);

        RuleFor(x => x.SubscriptionPlanId)
            .NotEqual(Guid.Empty)
            .WithMessage(MessageCodes.Fields.SUBSCRIPTION_PLAN_REQUIRED);

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .WithMessage(MessageCodes.Fields.BRANCH_NAME_REQUIRED)
            .MaximumLength(100)
            .WithMessage(MessageCodes.Fields.BRANCH_NAME_MAX_LENGTH)
            .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s'-]+$")
            .WithMessage(MessageCodes.Fields.BRANCH_NAME_INVALID_CHARACTERS);

        RuleFor(x => x.BranchAddress)
            .NotEmpty()
            .WithMessage(MessageCodes.Fields.BRANCH_ADDRESS_REQUIRED)
            .MaximumLength(200)
            .WithMessage(MessageCodes.Fields.BRANCH_ADDRESS_MAX_LENGTH);

        RuleFor(x => x.CountryId)
            .GreaterThan(0)
            .WithMessage(MessageCodes.Fields.COUNTRY_REQUIRED);

        RuleFor(x => x.CityId)
            .GreaterThan(0)
            .WithMessage(MessageCodes.Fields.CITY_REQUIRED);

        RuleFor(x => x.BranchPhoneNumbers)
            .NotEmpty()
            .WithMessage(MessageCodes.Fields.BRANCH_PHONE_NUMBERS_REQUIRED);

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
