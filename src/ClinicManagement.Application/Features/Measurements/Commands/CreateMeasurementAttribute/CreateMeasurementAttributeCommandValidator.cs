using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;

public class CreateMeasurementAttributeCommandValidator : AbstractValidator<CreateMeasurementAttributeCommand>
{
    public CreateMeasurementAttributeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(MessageCodes.Measurement.NAME_REQUIRED)
            .MaximumLength(100).WithErrorCode(MessageCodes.Measurement.NAME_TOO_LONG);

        RuleFor(x => x.DataType)
            .IsInEnum().WithErrorCode(MessageCodes.Measurement.INVALID_DATA_TYPE);
    }
}
