using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Fields.FULL_NAME_REQUIRED)
            .MaximumLength(100)
            .WithErrorCode(MessageCodes.Fields.FULL_NAME_MAX_LENGTH);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithErrorCode(MessageCodes.Fields.PHONE_NUMBER_MAX_LENGTH)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
