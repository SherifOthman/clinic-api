using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using FluentAssertions;

namespace ClinicManagement.Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void AddPayment_ToInvoice_ShouldUpdateTotals()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);
        var paymentId = Guid.NewGuid();

        // Act
        invoice.AddPayment(paymentId, 50m, PaymentMethod.Cash);

        // Assert
        invoice.Payments.Should().HaveCount(1);
        invoice.TotalPaid.Should().Be(50m);
        invoice.RemainingAmount.Should().Be(50m);
    }

    [Fact]
    public void AddPayment_MultiplePayments_ShouldAccumulateCorrectly()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);

        // Act
        invoice.AddPayment(Guid.NewGuid(), 30m, PaymentMethod.Cash);
        invoice.AddPayment(Guid.NewGuid(), 20m, PaymentMethod.CreditCard);
        invoice.AddPayment(Guid.NewGuid(), 50m, PaymentMethod.BankTransfer);

        // Assert
        invoice.Payments.Should().HaveCount(3);
        invoice.TotalPaid.Should().Be(100m);
        invoice.RemainingAmount.Should().Be(0m);
        invoice.IsFullyPaid.Should().BeTrue();
    }

    [Fact]
    public void AddPayment_ExceedingInvoiceAmount_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);

        // Act
        var act = () => invoice.AddPayment(Guid.NewGuid(), 150m, PaymentMethod.Cash);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*exceeds remaining*");
    }

    [Fact]
    public void AddPayment_ToFullyPaidInvoice_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);
        invoice.AddPayment(Guid.NewGuid(), 100m, PaymentMethod.Cash);

        // Act
        var act = () => invoice.AddPayment(Guid.NewGuid(), 10m, PaymentMethod.Cash);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*exceeds remaining*");
    }

    [Fact]
    public void AddPayment_WithDifferentPaymentMethods_ShouldTrackCorrectly()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);

        // Act
        invoice.AddPayment(Guid.NewGuid(), 40m, PaymentMethod.Cash);
        invoice.AddPayment(Guid.NewGuid(), 30m, PaymentMethod.CreditCard);
        invoice.AddPayment(Guid.NewGuid(), 30m, PaymentMethod.BankTransfer);

        // Assert
        var cashPayments = invoice.Payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
        var cardPayments = invoice.Payments.Where(p => p.PaymentMethod == PaymentMethod.CreditCard).Sum(p => p.Amount);
        var bankPayments = invoice.Payments.Where(p => p.PaymentMethod == PaymentMethod.BankTransfer).Sum(p => p.Amount);

        cashPayments.Should().Be(40m);
        cardPayments.Should().Be(30m);
        bankPayments.Should().Be(30m);
    }

    [Fact]
    public void AddPayment_WithReferenceNumber_ShouldStoreReference()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);
        var paymentId = Guid.NewGuid();
        var referenceNumber = "REF-12345";

        // Act
        invoice.AddPayment(paymentId, 50m, PaymentMethod.BankTransfer, referenceNumber);

        // Assert
        var payment = invoice.Payments.First();
        payment.ReferenceNumber.Should().Be(referenceNumber);
    }

    [Fact]
    public void AddPayment_PartialPayment_ShouldSetStatusToPartiallyPaid()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);

        // Act
        invoice.AddPayment(Guid.NewGuid(), 50m, PaymentMethod.Cash);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        invoice.IsPartiallyPaid.Should().BeTrue();
        invoice.IsFullyPaid.Should().BeFalse();
    }

    [Fact]
    public void AddPayment_FullPayment_ShouldSetStatusToFullyPaid()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);

        // Act
        invoice.AddPayment(Guid.NewGuid(), 100m, PaymentMethod.Cash);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.FullyPaid);
        invoice.IsFullyPaid.Should().BeTrue();
        invoice.RemainingAmount.Should().Be(0m);
    }

    [Fact]
    public void AddPayment_ShouldSetPaymentDate()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);
        var beforePayment = DateTime.UtcNow;

        // Act
        invoice.AddPayment(Guid.NewGuid(), 50m, PaymentMethod.Cash);

        // Assert
        var payment = invoice.Payments.First();
        payment.PaymentDate.Should().BeCloseTo(beforePayment, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddPayment_WithZeroAmount_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);

        // Act
        var act = () => invoice.AddPayment(Guid.NewGuid(), 0m, PaymentMethod.Cash);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*must be positive*");
    }

    [Fact]
    public void AddPayment_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);

        // Act
        var act = () => invoice.AddPayment(Guid.NewGuid(), -50m, PaymentMethod.Cash);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*must be positive*");
    }

    [Fact]
    public void TotalPaid_WithNoPayments_ShouldBeZero()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);

        // Assert
        invoice.TotalPaid.Should().Be(0m);
        invoice.RemainingAmount.Should().Be(100m);
    }

    [Fact]
    public void RemainingAmount_AfterDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var invoice = CreateInvoiceWithItems(100m);
        invoice.ApplyDiscount(20m);

        // Act
        invoice.AddPayment(Guid.NewGuid(), 30m, PaymentMethod.Cash);

        // Assert
        invoice.FinalAmount.Should().Be(80m);
        invoice.TotalPaid.Should().Be(30m);
        invoice.RemainingAmount.Should().Be(50m);
    }

    private static Invoice CreateInvoiceWithItems(decimal totalAmount)
    {
        var invoice = Invoice.Create(
            "INV-2026-000001",
            Guid.NewGuid(),
            Guid.NewGuid());

        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: totalAmount);
        invoice.Issue();

        return invoice;
    }
}
