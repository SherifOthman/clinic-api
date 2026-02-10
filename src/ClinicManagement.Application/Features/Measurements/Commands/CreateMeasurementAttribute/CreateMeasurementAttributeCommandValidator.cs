using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;

public class CreateMeasurementAttributeCommandValidator : AbstractValidator<CreateMeasurementAttributeCommand>
{
    public CreateMeasurementAttributeCommandValidator()
    {
        RuleFor(x => x.NameEn)
            .NotEmpty().WithErrorCode(MessageCodes.Measurement.NAME_REQUIRED)
            .MaximumLength(100).WithErrorCode(MessageCodes.Measurement.NAME_TOO_LONG);

        RuleFor(x => x.NameAr)
            .NotEmpty().WithErrorCode(MessageCodes.Measurement.NAME_REQUIRED)
            .MaximumLength(100).WithErrorCode(MessageCodes.Measurement.NAME_TOO_LONG);

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(500).WithErrorCode(MessageCodes.Measurement.DESCRIPTION_TOO_LONG)
            .When(x => !string.IsNullOrEmpty(x.DescriptionEn));

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(500).WithErrorCode(MessageCodes.Measurement.DESCRIPTION_TOO_LONG)
            .When(x => !string.IsNullOrEmpty(x.DescriptionAr));

        RuleFor(x => x.DataType)
            .IsInEnum().WithErrorCode(MessageCodes.Measurement.INVALID_DATA_TYPE);
    }
}