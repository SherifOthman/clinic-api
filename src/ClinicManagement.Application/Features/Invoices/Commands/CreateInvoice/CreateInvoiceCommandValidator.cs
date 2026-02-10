using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithErrorCode(MessageCodes.Invoice.PATIENT_REQUIRED);

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithErrorCode(MessageCodes.Invoice.DISCOUNT_CANNOT_BE_NEGATIVE);

        RuleFor(x => x.Items)
            .NotEmpty().WithErrorCode(MessageCodes.Invoice.EMPTY_ITEMS)
            .Must(items => items.Count > 0).WithErrorCode(MessageCodes.Invoice.EMPTY_ITEMS);

        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemCommandValidator());
    }
}