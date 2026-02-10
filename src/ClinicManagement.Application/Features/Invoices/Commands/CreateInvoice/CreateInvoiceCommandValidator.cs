using FluentValidation;

namespace ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient is required");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Invoice must have at least one item")
            .Must(items => items.Count > 0).WithMessage("Invoice must have at least one item");

        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemCommandValidator());
    }
}