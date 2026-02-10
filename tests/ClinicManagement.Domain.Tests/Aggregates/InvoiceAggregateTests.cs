using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Aggregates;

public class InvoiceAggregateTests
{
    private static Invoice CreateTestInvoice()
    {
        return Invoice.Create(
            "INV-2026-001",
            Guid.NewGuid(),
            Guid.NewGuid());
    }

    [Fact]
    public void Create_ValidData_ShouldCreateInvoice()
    {
        // Arrange
        var invoiceNumber = "INV-2026-001";
        var clinicId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        // Act
        var invoice = Invoice.Create(invoiceNumber, clinicId, patientId);

        // Assert
        invoice.Should().NotBeNull();
        invoice.InvoiceNumber.Should().Be(invoiceNumber);
        invoice.ClinicId.Should().Be(clinicId);
        invoice.PatientId.Should().Be(patientId);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.Items.Should().BeEmpty();
        invoice.Payments.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmptyInvoiceNumber_ShouldThrow()
    {
        // Act
        Action act = () => Invoice.Create("", Guid.NewGuid(), Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("Invoice number is required");
    }

    [Fact]
    public void Create_EmptyClinicId_ShouldThrow()
    {
        // Act
        Action act = () => Invoice.Create("INV-001", Guid.Empty, Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("Clinic ID is required");
    }

    [Fact]
    public void AddItem_ValidItem_ShouldAddToCollection()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        var medicineId = Guid.NewGuid();

        // Act
        invoice.AddItem(medicineId: medicineId, quantity: 2, unitPrice: 50);

        // Assert
        invoice.Items.Should().HaveCount(1);
        var item = invoice.Items.First();
        item.MedicineId.Should().Be(medicineId);
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(50);
        invoice.SubtotalAmount.Should().Be(100);
    }

    [Fact]
    public void AddItem_MultipleItems_ShouldCalculateTotal()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 2, unitPrice: 50);
        invoice.AddItem(medicalServiceId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        invoice.AddItem(medicalSupplyId: Guid.NewGuid(), quantity: 3, unitPrice: 25);

        // Assert
        invoice.Items.Should().HaveCount(3);
        invoice.SubtotalAmount.Should().Be(275); // (2*50) + (1*100) + (3*25)
    }

    [Fact]
    public void AddItem_NoItemTypeSpecified_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        Action act = () => invoice.AddItem(quantity: 1, unitPrice: 100);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("Exactly one of MedicalServiceId, MedicineId, or MedicalSupplyId must be specified");
    }

    [Fact]
    public void AddItem_MultipleItemTypesSpecified_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        Action act = () => invoice.AddItem(
            medicineId: Guid.NewGuid(),
            medicalServiceId: Guid.NewGuid(),
            quantity: 1,
            unitPrice: 100);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("Exactly one of MedicalServiceId, MedicineId, or MedicalSupplyId must be specified");
    }

    [Fact]
    public void AddItem_ZeroQuantity_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        Action act = () => invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 0, unitPrice: 100);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("Quantity must be positive");
    }

    [Fact]
    public void AddItem_NegativePrice_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        Action act = () => invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: -10);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("Unit price cannot be negative");
    }

    [Fact]
    public void AddItem_WhenNotDraft_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        invoice.Issue();

        // Act
        Action act = () => invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 50);

        // Assert
        act.Should().Throw<InvalidInvoiceStateException>();
    }

    [Fact]
    public void RemoveItem_ExistingItem_ShouldRemoveFromCollection()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        var itemId = invoice.Items.First().Id;

        // Act
        invoice.RemoveItem(itemId);

        // Assert
        invoice.Items.Should().BeEmpty();
        invoice.SubtotalAmount.Should().Be(0);
    }

    [Fact]
    public void RemoveItem_NonExistentItem_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        Action act = () => invoice.RemoveItem(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("Item not found");
    }

    [Fact]
    public void ApplyDiscount_ValidAmount_ShouldSetDiscount()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);

        // Act
        invoice.ApplyDiscount(20);

        // Assert
        invoice.Discount.Should().Be(20);
        invoice.SubtotalAmount.Should().Be(100);
        invoice.FinalAmount.Should().Be(80);
        invoice.DiscountPercentage.Should().Be(20);
    }

    [Fact]
    public void ApplyDiscount_NegativeAmount_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);

        // Act
        Action act = () => invoice.ApplyDiscount(-10);

        // Assert
        act.Should().Throw<InvalidDiscountException>();
    }

    [Fact]
    public void ApplyDiscount_ExceedsSubtotal_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);

        // Act
        Action act = () => invoice.ApplyDiscount(150);

        // Assert
        act.Should().Throw<InvalidDiscountException>();
    }

    [Fact]
    public void ApplyPercentageDiscount_ValidPercentage_ShouldCalculateDiscount()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);

        // Act
        invoice.ApplyPercentageDiscount(15);

        // Assert
        invoice.Discount.Should().Be(15);
        invoice.FinalAmount.Should().Be(85);
    }

    [Fact]
    public void SetTax_ValidAmount_ShouldSetTax()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);

        // Act
        invoice.SetTax(15);

        // Assert
        invoice.TaxAmount.Should().Be(15);
        invoice.FinalAmount.Should().Be(115);
    }

    [Fact]
    public void Issue_WithItems_ShouldChangeStatus()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);

        // Act
        invoice.Issue();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Issued);
        invoice.IssuedDate.Should().NotBeNull();
        invoice.DueDate.Should().NotBeNull();
    }

    [Fact]
    public void Issue_WithoutItems_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        Action act = () => invoice.Issue();

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("Cannot issue an invoice without items");
    }

    [Fact]
    public void Issue_WhenNotDraft_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        invoice.Issue();

        // Act
        Action act = () => invoice.Issue();

        // Assert
        act.Should().Throw<InvalidInvoiceStateException>();
    }

    [Fact]
    public void AddPayment_ValidAmount_ShouldAddPayment()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        invoice.Issue();

        // Act
        invoice.AddPayment(Guid.NewGuid(), 50, PaymentMethod.Cash);

        // Assert
        invoice.Payments.Should().HaveCount(1);
        invoice.TotalPaid.Should().Be(50);
        invoice.RemainingAmount.Should().Be(50);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void AddPayment_FullAmount_ShouldMarkAsFullyPaid()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        invoice.Issue();

        // Act
        invoice.AddPayment(Guid.NewGuid(), 100, PaymentMethod.Cash);

        // Assert
        invoice.TotalPaid.Should().Be(100);
        invoice.RemainingAmount.Should().Be(0);
        invoice.IsFullyPaid.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.FullyPaid);
    }

    [Fact]
    public void AddPayment_ExceedingRemaining_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        invoice.Issue();

        // Act
        Action act = () => invoice.AddPayment(Guid.NewGuid(), 150, PaymentMethod.Cash);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*exceeds remaining amount*");
    }

    [Fact]
    public void AddPayment_WhenCancelled_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        invoice.Cancel("Test cancellation");

        // Act
        Action act = () => invoice.AddPayment(Guid.NewGuid(), 50, PaymentMethod.Cash);

        // Assert
        act.Should().Throw<InvalidInvoiceStateException>();
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldChangeStatus()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);

        // Act
        invoice.Cancel("Customer request");

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
        invoice.Notes.Should().Contain("Cancelled: Customer request");
    }

    [Fact]
    public void Cancel_WhenFullyPaid_ShouldThrow()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddItem(medicineId: Guid.NewGuid(), quantity: 1, unitPrice: 100);
        invoice.Issue();
        invoice.AddPayment(Guid.NewGuid(), 100, PaymentMethod.Cash);

        // Act
        Action act = () => invoice.Cancel("Test");

        // Assert
        act.Should().Throw<InvalidInvoiceStateException>();
    }

    [Fact]
    public void UpdateNotes_ShouldSetNotes()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        invoice.UpdateNotes("Test notes");

        // Assert
        invoice.Notes.Should().Be("Test notes");
    }

    [Fact]
    public void Items_ShouldBeReadOnly()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Assert
        invoice.Items.Should().BeAssignableTo<IReadOnlyCollection<InvoiceItem>>();
    }

    [Fact]
    public void Payments_ShouldBeReadOnly()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Assert
        invoice.Payments.Should().BeAssignableTo<IReadOnlyCollection<Payment>>();
    }
}
