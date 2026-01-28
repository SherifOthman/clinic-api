using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;

    public UpdatePatientCommandValidator(IPhoneNumberValidationService phoneNumberValidationService)
    {
        _phoneNumberValidationService = phoneNumberValidationService;

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Patient ID must be a positive number");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Full name must be less than 200 characters")
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'-]+$").WithMessage("Full name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Date of birth cannot be more than 150 years ago")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Please select a valid gender")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.PhoneNumbers)
            .NotEmpty().WithMessage("At least one phone number is required");

        RuleForEach(x => x.PhoneNumbers)
            .SetValidator(new UpdatePatientPhoneNumberValidator(_phoneNumberValidationService));

        RuleFor(x => x.ChronicDiseaseIds)
            .Must(ids => ids.All(id => id > 0)).WithMessage("All chronic disease IDs must be positive numbers")
            .When(x => x.ChronicDiseaseIds.Any());
    }
}

public class UpdatePatientPhoneNumberValidator : AbstractValidator<UpdatePatientPhoneNumberDto>
{
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;

    public UpdatePatientPhoneNumberValidator(IPhoneNumberValidationService phoneNumberValidationService)
    {
        _phoneNumberValidationService = phoneNumberValidationService;

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Phone number ID must be a positive number")
            .When(x => x.Id.HasValue);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Must(BeValidPhoneNumber).WithMessage("Please enter a valid phone number");
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