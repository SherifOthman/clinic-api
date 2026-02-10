using FluentValidation;

namespace ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;

public class CreateMeasurementAttributeCommandValidator : AbstractValidator<CreateMeasurementAttributeCommand>
{
    public CreateMeasurementAttributeCommandValidator()
    {
        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required")
            .MaximumLength(100).WithMessage("English name cannot exceed 100 characters");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Arabic name is required")
            .MaximumLength(100).WithMessage("Arabic name cannot exceed 100 characters");

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(500).WithMessage("English description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DescriptionEn));

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(500).WithMessage("Arabic description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DescriptionAr));

        RuleFor(x => x.DataType)
            .IsInEnum().WithMessage("Invalid data type");
    }
}