using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    private readonly IPhoneValidationService _phoneValidationService;

    public CompleteOnboardingCommandValidator(IPhoneValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;

        RuleFor(x => x.Dto.ClinicName)
            .NotEmpty().WithErrorCode(MessageCodes.Validation.REQUIRED_FIELD)
            .MaximumLength(200).WithErrorCode(MessageCodes.Validation.INVALID_LENGTH);

        RuleFor(x => x.Dto.BranchName)
            .NotEmpty().WithErrorCode(MessageCodes.Validation.REQUIRED_FIELD)
            .MaximumLength(200).WithErrorCode(MessageCodes.Validation.INVALID_LENGTH);

        RuleFor(x => x.Dto.BranchAddress)
            .NotEmpty().WithErrorCode(MessageCodes.Validation.REQUIRED_FIELD)
            .MaximumLength(500).WithErrorCode(MessageCodes.Validation.INVALID_LENGTH);

        RuleFor(x => x.Dto.BranchPhoneNumbers)
            .NotEmpty().WithErrorCode(MessageCodes.Validation.REQUIRED_FIELD)
            .Must(phones => phones != null && phones.Count > 0)
            .WithErrorCode(MessageCodes.Validation.REQUIRED_FIELD);

        RuleForEach(x => x.Dto.BranchPhoneNumbers)
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