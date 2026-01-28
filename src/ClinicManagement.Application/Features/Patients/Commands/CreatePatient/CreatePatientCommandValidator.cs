using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;

    public CreatePatientCommandValidator(IPhoneNumberValidationService phoneNumberValidationService)
    {
        _phoneNumberValidationService = phoneNumberValidationService;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(MessageCodes.Fields.FULL_NAME_REQUIRED)
            .MinimumLength(2).WithMessage(MessageCodes.Fields.FULL_NAME_MIN_LENGTH)
            .MaximumLength(200).WithMessage(MessageCodes.Fields.FULL_NAME_MAX_LENGTH)
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'-]+$").WithMessage(MessageCodes.Fields.FULL_NAME_INVALID_CHARACTERS);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage(MessageCodes.Fields.DATE_OF_BIRTH_PAST)
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage(MessageCodes.Fields.DATE_OF_BIRTH_TOO_OLD)
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage(MessageCodes.Fields.GENDER_INVALID)
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.PhoneNumbers)
            .NotEmpty().WithMessage(MessageCodes.Fields.PHONE_NUMBERS_REQUIRED);

        RuleForEach(x => x.PhoneNumbers)
            .SetValidator(new CreatePatientPhoneNumberValidator(_phoneNumberValidationService));

        RuleFor(x => x.ChronicDiseaseIds)
            .Must(ids => ids.All(id => id > 0)).WithMessage(MessageCodes.Fields.CHRONIC_DISEASE_IDS_POSITIVE)
            .When(x => x.ChronicDiseaseIds.Any());
    }
}

public class CreatePatientPhoneNumberValidator : AbstractValidator<CreatePatientPhoneNumberDto>
{
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;

    public CreatePatientPhoneNumberValidator(IPhoneNumberValidationService phoneNumberValidationService)
    {
        _phoneNumberValidationService = phoneNumberValidationService;

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(MessageCodes.Fields.PHONE_NUMBER_REQUIRED)
            .Must(BeValidPhoneNumber).WithMessage(MessageCodes.Fields.PHONE_NUMBER_INVALID);
    }

    private bool BeValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return false;

        // For Egyptian numbers, add default country code if missing
        var processedNumber = phoneNumber;
        if (phoneNumber.StartsWith("01") && phoneNumber.Length == 11)
        {
            processedNumber = "+20" + phoneNumber;
        }
        else if (phoneNumber.StartsWith("1") && phoneNumber.Length == 10)
        {
            processedNumber = "+201" + phoneNumber;
        }

        return _phoneNumberValidationService.IsValidPhoneNumber(processedNumber, "EG");
    }
}