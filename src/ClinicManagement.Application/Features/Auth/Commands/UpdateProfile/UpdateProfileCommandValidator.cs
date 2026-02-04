using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Fields.FIRST_NAME_REQUIRED)
            .MaximumLength(50)
            .WithErrorCode(MessageCodes.Fields.FIRST_NAME_MAX_LENGTH);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Fields.LAST_NAME_REQUIRED)
            .MaximumLength(50)
            .WithErrorCode(MessageCodes.Fields.LAST_NAME_MAX_LENGTH);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithErrorCode(MessageCodes.Fields.PHONE_NUMBER_MAX_LENGTH)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(500)
            .WithErrorCode(MessageCodes.Fields.PROFILE_IMAGE_URL_MAX_LENGTH)
            .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl));
    }
}
