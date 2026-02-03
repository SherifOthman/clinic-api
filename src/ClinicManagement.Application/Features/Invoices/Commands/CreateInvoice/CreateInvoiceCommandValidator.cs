using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.ClinicPatientId)
            .NotEmpty().WithErrorCode(MessageCodes.Invoice.PATIENT_REQUIRED);

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithErrorCode(MessageCodes.Invoice.DISCOUNT_CANNOT_BE_NEGATIVE);

        RuleFor(x => x.Items)
            .NotEmpty().WithErrorCode(MessageCodes.Invoice.EMPTY_ITEMS)
            .Must(items => items.Count > 0).WithErrorCode(MessageCodes.Invoice.EMPTY_ITEMS);

        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemCommandValidator());
    }
}

public class CreateInvoiceItemCommandValidator : AbstractValidator<CreateInvoiceItemCommand>
{
    public CreateInvoiceItemCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithErrorCode(MessageCodes.Invoice.INVALID_ITEM_QUANTITY);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithErrorCode(MessageCodes.Invoice.INVALID_ITEM_PRICE);

        RuleFor(x => x)
            .Must(item => item.MedicalServiceId.HasValue || item.MedicineId.HasValue || item.MedicalSupplyId.HasValue)
            .WithErrorCode("INVOICE.ITEM.REQUIRED");
    }
}
