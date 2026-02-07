using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    private readonly IPhoneValidationService _phoneValidationService;

    public CompleteOnboardingCommandValidator(IPhoneValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;

        RuleFor(x => x.ClinicName)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.CLINIC_NAME_REQUIRED)
            .MaximumLength(200).WithErrorCode(MessageCodes.Fields.CLINIC_NAME_MAX_LENGTH);

        RuleFor(x => x.BranchName)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.BRANCH_NAME_REQUIRED)
            .MaximumLength(200).WithErrorCode(MessageCodes.Fields.BRANCH_NAME_MAX_LENGTH);

        RuleFor(x => x.BranchAddress)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.BRANCH_ADDRESS_REQUIRED)
            .MaximumLength(500).WithErrorCode(MessageCodes.Fields.BRANCH_ADDRESS_MAX_LENGTH);

        RuleFor(x => x.BranchPhoneNumbers)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.PHONE_NUMBERS_REQUIRED)
            .Must(phones => phones != null && phones.Count > 0)
            .WithErrorCode(MessageCodes.Fields.PHONE_NUMBERS_REQUIRED);

        RuleForEach(x => x.BranchPhoneNumbers)
            .ChildRules(phone =>
            {
                phone.RuleFor(p => p.PhoneNumber)
                    .NotEmpty().WithErrorCode(MessageCodes.Fields.PHONE_NUMBER_REQUIRED)
                    .Must(BeValidPhoneNumber).WithErrorCode(MessageCodes.Fields.PHONE_NUMBER_INVALID);
            });
    }

    private bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return false;
        
        var result = _phoneValidationService.ValidatePhoneNumber(phoneNumber);
        return result.IsValid;
    }
}
