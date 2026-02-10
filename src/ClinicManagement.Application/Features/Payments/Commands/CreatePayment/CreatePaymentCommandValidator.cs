using FluentValidation;

namespace ClinicManagement.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Note cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Note));
    }
}