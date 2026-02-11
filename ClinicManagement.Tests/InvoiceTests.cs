using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using FluentAssertions;

namespace ClinicManagement.Tests.Domain;

public class InvoiceTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateInvoice()
    {
        // Arrange
        var invoiceNumber = "INV-2026-000001";
        var clinicId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        // Act
        var invoice = Invoice.Create(invoiceNumber, clinicId, patientId);

        // Assert
        invoice.Should().NotBeNull();
        invoice.InvoiceNumber.Should().Be(invoiceNumber);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
    }

    [Fact]
    public void AddItem_WithValidData_ShouldAddItem()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        var serviceId = Guid.NewGuid();

        // Act
        invoice.AddItem(medicalServiceId: serviceId, quantity: 1, unitPrice: 100m);

        // Assert
        invoice.Items.Should().HaveCount(1);
        invoice.SubtotalAmount.Should().Be(100m);
    }

    [Fact]
    public void AddItem_WhenNotDraft_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);
        invoice.Issue();

        // Act
        var act = () => invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 50m);

        // Assert
        act.Should().Throw<InvalidInvoiceStateException>();
    }

    [Fact]
    public void ApplyDiscount_WithValidAmount_ShouldApplyDiscount()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);

        // Act
        invoice.ApplyDiscount(20m);

        // Assert
        invoice.Discount.Should().Be(20m);
        invoice.FinalAmount.Should().Be(80m);
    }

    [Fact]
    public void ApplyDiscount_ExceedingSubtotal_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);

        // Act
        var act = () => invoice.ApplyDiscount(150m);

        // Assert
        act.Should().Throw<InvalidDiscountException>();
    }

    [Fact]
    public void ApplyPercentageDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);

        // Act
        invoice.ApplyPercentageDiscount(10m); // 10%

        // Assert
        invoice.Discount.Should().Be(10m);
        invoice.FinalAmount.Should().Be(90m);
        invoice.DiscountPercentage.Should().Be(10m);
    }

    [Fact]
    public void Issue_WithItems_ShouldChangeStatusToIssued()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);

        // Act
        invoice.Issue();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Issued);
        invoice.IssuedDate.Should().NotBeNull();
        invoice.DueDate.Should().NotBeNull();
    }

    [Fact]
    public void Issue_WithoutItems_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateValidInvoice();

        // Act
        var act = () => invoice.Issue();

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*without items*");
    }

    [Fact]
    public void AddPayment_WithValidAmount_ShouldAddPayment()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);
        invoice.Issue();

        // Act
        invoice.AddPayment(Guid.NewGuid(), 50m, PaymentMethod.Cash);

        // Assert
        invoice.Payments.Should().HaveCount(1);
        invoice.TotalPaid.Should().Be(50m);
        invoice.RemainingAmount.Should().Be(50m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void AddPayment_FullAmount_ShouldMarkAsFullyPaid()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);
        invoice.Issue();

        // Act
        invoice.AddPayment(Guid.NewGuid(), 100m, PaymentMethod.Cash);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.FullyPaid);
        invoice.IsFullyPaid.Should().BeTrue();
        invoice.RemainingAmount.Should().Be(0m);
    }

    [Fact]
    public void AddPayment_ExceedingRemaining_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);
        invoice.Issue();

        // Act
        var act = () => invoice.AddPayment(Guid.NewGuid(), 150m, PaymentMethod.Cash);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*exceeds remaining*");
    }

    [Fact]
    public void Cancel_WhenNotFullyPaid_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);
        invoice.Issue();

        // Act
        invoice.Cancel("Customer request");

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenFullyPaid_ShouldThrowException()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);
        invoice.Issue();
        invoice.AddPayment(Guid.NewGuid(), 100m, PaymentMethod.Cash);

        // Act
        var act = () => invoice.Cancel();

        // Assert
        act.Should().Throw<InvalidInvoiceStateException>();
    }

    [Fact]
    public void IsOverdue_WhenPastDueDate_ShouldReturnTrue()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100m);
        invoice.Issue();
        
        // Manually set due date to past (using reflection for testing)
        var dueDate = DateTime.UtcNow.AddDays(-5);
        typeof(Invoice).GetProperty("DueDate")!.SetValue(invoice, dueDate);

        // Act & Assert
        invoice.IsOverdue.Should().BeTrue();
        invoice.DaysOverdue.Should().Be(5);
    }

    private static Invoice CreateValidInvoice()
    {
        return Invoice.Create(
            "INV-2026-000001",
            Guid.NewGuid(),
            Guid.NewGuid());
    }
}
