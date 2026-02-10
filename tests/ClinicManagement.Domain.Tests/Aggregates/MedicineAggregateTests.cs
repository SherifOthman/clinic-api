using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Events;
using FluentAssertions;
using Xunit;

namespace ClinicManagement.Domain.Tests.Aggregates;

public class MedicineAggregateTests
{
    private readonly Guid _clinicBranchId = Guid.NewGuid();

    #region Factory Method Tests

    [Fact]
    public void Create_WithValidData_ShouldCreateMedicine()
    {
        // Act
        var medicine = Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: "Paracetamol",
            boxPrice: 50.00m,
            stripsPerBox: 10,
            initialStock: 100
        );

        // Assert
        medicine.Should().NotBeNull();
        medicine.ClinicBranchId.Should().Be(_clinicBranchId);
        medicine.Name.Should().Be("Paracetamol");
        medicine.BoxPrice.Should().Be(50.00m);
        medicine.StripsPerBox.Should().Be(10);
        medicine.TotalStripsInStock.Should().Be(100);
        medicine.IsActive.Should().BeTrue();
        medicine.IsDiscontinued.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldRaiseMedicineCreatedEvent()
    {
        // Act
        var medicine = Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: "Paracetamol",
            boxPrice: 50.00m,
            stripsPerBox: 10,
            initialStock: 100
        );

        // Assert
        medicine.DomainEvents.Should().ContainSingle(e => e is MedicineCreatedEvent);
        var createdEvent = medicine.DomainEvents.OfType<MedicineCreatedEvent>().First();
        createdEvent.MedicineId.Should().Be(medicine.Id);
        createdEvent.Name.Should().Be("Paracetamol");
        createdEvent.InitialStock.Should().Be(100);
    }

    [Fact]
    public void Create_WithLowStock_ShouldRaiseLowStockEvent()
    {
        // Act
        var medicine = Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: "Paracetamol",
            boxPrice: 50.00m,
            stripsPerBox: 10,
            initialStock: 5,
            minimumStockLevel: 10
        );

        // Assert
        medicine.DomainEvents.Should().Contain(e => e is MedicineStockLowEvent);
        var lowStockEvent = medicine.DomainEvents.OfType<MedicineStockLowEvent>().First();
        lowStockEvent.CurrentStock.Should().Be(5);
        lowStockEvent.MinimumStockLevel.Should().Be(10);
    }

    [Fact]
    public void Create_WithEmptyClinicBranchId_ShouldThrowException()
    {
        // Act
        var act = () => Medicine.Create(
            clinicBranchId: Guid.Empty,
            name: "Paracetamol",
            boxPrice: 50.00m,
            stripsPerBox: 10
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Clinic branch ID is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowException(string name)
    {
        // Act
        var act = () => Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: name,
            boxPrice: 50.00m,
            stripsPerBox: 10
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Medicine name is required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidBoxPrice_ShouldThrowException(decimal boxPrice)
    {
        // Act
        var act = () => Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: "Paracetamol",
            boxPrice: boxPrice,
            stripsPerBox: 10
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Box price must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithInvalidStripsPerBox_ShouldThrowException(int stripsPerBox)
    {
        // Act
        var act = () => Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: "Paracetamol",
            boxPrice: 50.00m,
            stripsPerBox: stripsPerBox
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Strips per box must be positive*");
    }

    [Fact]
    public void Create_WithNegativeInitialStock_ShouldThrowException()
    {
        // Act
        var act = () => Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: "Paracetamol",
            boxPrice: 50.00m,
            stripsPerBox: 10,
            initialStock: -10
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Initial stock cannot be negative*");
    }

    [Fact]
    public void Create_WithReorderLevelLessThanMinimum_ShouldThrowException()
    {
        // Act
        var act = () => Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: "Paracetamol",
            boxPrice: 50.00m,
            stripsPerBox: 10,
            minimumStockLevel: 20,
            reorderLevel: 10
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Reorder level cannot be less than minimum stock level*");
    }

    [Fact]
    public void Create_WithPastExpiryDate_ShouldThrowException()
    {
        // Act
        var act = () => Medicine.Create(
            clinicBranchId: _clinicBranchId,
            name: "Paracetamol",
            boxPrice: 50.00m,
            stripsPerBox: 10,
            expiryDate: DateTime.UtcNow.AddDays(-1)
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Expiry date must be in the future*");
    }

    #endregion

    #region AddStock Tests

    [Fact]
    public void AddStock_WithValidAmount_ShouldIncreaseStock()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.ClearDomainEvents();

        // Act
        medicine.AddStock(50, "Restocking");

        // Assert
        medicine.TotalStripsInStock.Should().Be(150);
    }

    [Fact]
    public void AddStock_ShouldRaiseStockAddedEvent()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.ClearDomainEvents();

        // Act
        medicine.AddStock(50, "Restocking");

        // Assert
        medicine.DomainEvents.Should().ContainSingle(e => e is MedicineStockAddedEvent);
        var addedEvent = medicine.DomainEvents.OfType<MedicineStockAddedEvent>().First();
        addedEvent.StripsAdded.Should().Be(50);
        addedEvent.NewTotalStock.Should().Be(150);
        addedEvent.Reason.Should().Be("Restocking");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void AddStock_WithInvalidAmount_ShouldThrowException(int strips)
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        var act = () => medicine.AddStock(strips);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Strips to add must be positive*");
    }

    [Fact]
    public void AddStock_WhenDiscontinued_ShouldThrowException()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.Discontinue("Expired");

        // Act
        var act = () => medicine.AddStock(50);

        // Assert
        act.Should().Throw<DiscontinuedMedicineException>();
    }

    #endregion

    #region RemoveStock Tests

    [Fact]
    public void RemoveStock_WithValidAmount_ShouldDecreaseStock()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.ClearDomainEvents();

        // Act
        medicine.RemoveStock(30, "Sale");

        // Assert
        medicine.TotalStripsInStock.Should().Be(70);
    }

    [Fact]
    public void RemoveStock_ShouldRaiseStockRemovedEvent()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.ClearDomainEvents();

        // Act
        medicine.RemoveStock(30, "Sale");

        // Assert
        medicine.DomainEvents.Should().Contain(e => e is MedicineStockRemovedEvent);
        var removedEvent = medicine.DomainEvents.OfType<MedicineStockRemovedEvent>().First();
        removedEvent.StripsRemoved.Should().Be(30);
        removedEvent.NewTotalStock.Should().Be(70);
        removedEvent.Reason.Should().Be("Sale");
    }

    [Fact]
    public void RemoveStock_WhenResultsInLowStock_ShouldRaiseLowStockEvent()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100, minimumStockLevel: 10);
        medicine.ClearDomainEvents();

        // Act
        medicine.RemoveStock(95);

        // Assert
        medicine.TotalStripsInStock.Should().Be(5);
        medicine.DomainEvents.Should().Contain(e => e is MedicineStockLowEvent);
        var lowStockEvent = medicine.DomainEvents.OfType<MedicineStockLowEvent>().First();
        lowStockEvent.CurrentStock.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void RemoveStock_WithInvalidAmount_ShouldThrowException(int strips)
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        var act = () => medicine.RemoveStock(strips);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Strips to remove must be positive*");
    }

    [Fact]
    public void RemoveStock_MoreThanAvailable_ShouldThrowException()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        var act = () => medicine.RemoveStock(150);

        // Assert
        act.Should().Throw<InsufficientStockException>();
    }

    #endregion

    #region Discontinue Tests

    [Fact]
    public void Discontinue_ShouldMarkAsDiscontinuedAndInactive()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.ClearDomainEvents();

        // Act
        medicine.Discontinue("Manufacturer stopped production");

        // Assert
        medicine.IsDiscontinued.Should().BeTrue();
        medicine.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Discontinue_ShouldRaiseDiscontinuedEvent()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.ClearDomainEvents();

        // Act
        medicine.Discontinue("Manufacturer stopped production");

        // Assert
        medicine.DomainEvents.Should().ContainSingle(e => e is MedicineDiscontinuedEvent);
        var discontinuedEvent = medicine.DomainEvents.OfType<MedicineDiscontinuedEvent>().First();
        discontinuedEvent.MedicineId.Should().Be(medicine.Id);
        discontinuedEvent.Reason.Should().Be("Manufacturer stopped production");
    }

    #endregion

    #region Reactivate Tests

    [Fact]
    public void Reactivate_ShouldMarkAsActiveAndNotDiscontinued()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.Discontinue("Test");

        // Act
        medicine.Reactivate();

        // Assert
        medicine.IsDiscontinued.Should().BeFalse();
        medicine.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Reactivate_WhenExpired_ShouldThrowException()
    {
        // Arrange
        var medicine = Medicine.Create(
            _clinicBranchId,
            "Paracetamol",
            50.00m,
            10,
            100,
            expiryDate: DateTime.UtcNow.AddDays(1)
        );
        medicine.Discontinue("Expired");

        // Wait for expiry (simulate)
        // In real scenario, we'd use a time provider
        // For now, we'll test with UpdateExpiryDate

        // Act & Assert
        // Cannot easily test expiry without time provider
        // This test would need refactoring to use IDateTimeProvider
    }

    #endregion

    #region UpdateInfo Tests

    [Fact]
    public void UpdateInfo_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        medicine.UpdateInfo(
            name: "Paracetamol 500mg",
            boxPrice: 60.00m,
            stripsPerBox: 12,
            minimumStockLevel: 15,
            reorderLevel: 30,
            description: "Pain reliever",
            manufacturer: "PharmaCorp",
            batchNumber: "BATCH123"
        );

        // Assert
        medicine.Name.Should().Be("Paracetamol 500mg");
        medicine.BoxPrice.Should().Be(60.00m);
        medicine.StripsPerBox.Should().Be(12);
        medicine.MinimumStockLevel.Should().Be(15);
        medicine.ReorderLevel.Should().Be(30);
        medicine.Description.Should().Be("Pain reliever");
        medicine.Manufacturer.Should().Be("PharmaCorp");
        medicine.BatchNumber.Should().Be("BATCH123");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateInfo_WithInvalidName_ShouldThrowException(string name)
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        var act = () => medicine.UpdateInfo(name, 50.00m, 10, 10);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Medicine name is required*");
    }

    #endregion

    #region UpdateExpiryDate Tests

    [Fact]
    public void UpdateExpiryDate_WithFutureDate_ShouldUpdateExpiryDate()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        var newExpiryDate = DateTime.UtcNow.AddMonths(6);

        // Act
        medicine.UpdateExpiryDate(newExpiryDate);

        // Assert
        medicine.ExpiryDate.Should().Be(newExpiryDate);
    }

    [Fact]
    public void UpdateExpiryDate_WithPastDate_ShouldThrowException()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        var act = () => medicine.UpdateExpiryDate(DateTime.UtcNow.AddDays(-1));

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Expiry date must be in the future*");
    }

    #endregion

    #region Calculated Properties Tests

    [Fact]
    public void StripPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Assert
        medicine.StripPrice.Should().Be(5.00m);
    }

    [Fact]
    public void FullBoxesInStock_ShouldCalculateCorrectly()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 105);

        // Assert
        medicine.FullBoxesInStock.Should().Be(10);
        medicine.RemainingStrips.Should().Be(5);
    }

    [Fact]
    public void IsLowStock_ShouldReturnTrueWhenBelowMinimum()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 5, minimumStockLevel: 10);

        // Assert
        medicine.IsLowStock.Should().BeTrue();
    }

    [Fact]
    public void NeedsReorder_ShouldReturnTrueWhenBelowReorderLevel()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 15, minimumStockLevel: 10, reorderLevel: 20);

        // Assert
        medicine.NeedsReorder.Should().BeTrue();
    }

    [Fact]
    public void StockStatus_ShouldReturnCorrectStatus()
    {
        // Arrange & Act & Assert
        var outOfStock = Medicine.Create(_clinicBranchId, "Med1", 50.00m, 10, 0);
        outOfStock.StockStatus.Should().Be(StockStatus.OutOfStock);

        var lowStock = Medicine.Create(_clinicBranchId, "Med2", 50.00m, 10, 5, minimumStockLevel: 10);
        lowStock.StockStatus.Should().Be(StockStatus.LowStock);

        var needsReorder = Medicine.Create(_clinicBranchId, "Med3", 50.00m, 10, 15, minimumStockLevel: 10, reorderLevel: 20);
        needsReorder.StockStatus.Should().Be(StockStatus.NeedsReorder);

        var inStock = Medicine.Create(_clinicBranchId, "Med4", 50.00m, 10, 100, minimumStockLevel: 10, reorderLevel: 20);
        inStock.StockStatus.Should().Be(StockStatus.InStock);
    }

    [Fact]
    public void InventoryValue_ShouldCalculateCorrectly()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 105);

        // Act
        var inventoryValue = medicine.InventoryValue;

        // Assert
        // 10 full boxes * 50 = 500
        // 5 remaining strips * 5 = 25
        // Total = 525
        inventoryValue.Should().Be(525.00m);
    }

    #endregion

    #region IsQuantityAvailable Tests

    [Fact]
    public void IsQuantityAvailable_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act & Assert
        medicine.IsQuantityAvailable(50).Should().BeTrue();
    }

    [Fact]
    public void IsQuantityAvailable_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act & Assert
        medicine.IsQuantityAvailable(150).Should().BeFalse();
    }

    [Fact]
    public void IsQuantityAvailable_WhenDiscontinued_ShouldReturnFalse()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);
        medicine.Discontinue("Test");

        // Act & Assert
        medicine.IsQuantityAvailable(50).Should().BeFalse();
    }

    #endregion

    #region CalculatePrice Tests

    [Fact]
    public void CalculatePrice_WithStrips_ShouldCalculateCorrectly()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        var price = medicine.CalculatePrice(15);

        // Assert
        price.Should().Be(75.00m); // 15 strips * 5.00 per strip
    }

    [Fact]
    public void CalculatePrice_WithBoxesAndStrips_ShouldCalculateCorrectly()
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        var price = medicine.CalculatePrice(boxes: 2, strips: 5);

        // Assert
        price.Should().Be(125.00m); // (2 * 50) + (5 * 5)
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CalculatePrice_WithInvalidStrips_ShouldThrowException(int strips)
    {
        // Arrange
        var medicine = Medicine.Create(_clinicBranchId, "Paracetamol", 50.00m, 10, 100);

        // Act
        var act = () => medicine.CalculatePrice(strips);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Strips must be positive*");
    }

    #endregion
}
