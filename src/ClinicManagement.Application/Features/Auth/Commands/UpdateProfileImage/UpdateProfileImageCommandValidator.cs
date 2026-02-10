using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandValidator : AbstractValidator<UpdateProfileImageCommand>
{
    public UpdateProfileImageCommandValidator()
    {
        RuleFor(x => x.ProfileImageUrl)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Fields.PROFILE_IMAGE_URL_REQUIRED)
            .MaximumLength(500)
            .WithErrorCode(MessageCodes.Fields.PROFILE_IMAGE_URL_MAX_LENGTH);
    }
}