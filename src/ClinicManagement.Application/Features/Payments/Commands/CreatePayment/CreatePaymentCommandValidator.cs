using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithErrorCode(MessageCodes.Payment.INVOICE_REQUIRED);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithErrorCode(MessageCodes.Payment.AMOUNT_MUST_BE_POSITIVE);

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithErrorCode(MessageCodes.Validation.REQUIRED_FIELD);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithErrorCode(MessageCodes.Payment.INVALID_PAYMENT_METHOD);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithErrorCode(MessageCodes.Payment.REFERENCE_NUMBER_TOO_LONG)
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.Note)
            .MaximumLength(500).WithErrorCode(MessageCodes.Payment.NOTE_TOO_LONG)
            .When(x => !string.IsNullOrEmpty(x.Note));
    }
}