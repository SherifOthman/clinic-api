using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using FluentAssertions;

namespace ClinicManagement.Tests.Domain;

public class MedicineTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateMedicine()
    {
        // Arrange
        var clinicBranchId = Guid.NewGuid();
        var name = "Paracetamol";
        var boxPrice = 100m;
        var stripsPerBox = 10;

        // Act
        var medicine = Medicine.Create(
            clinicBranchId,
            name,
            boxPrice,
            stripsPerBox);

        // Assert
        medicine.Should().NotBeNull();
        medicine.Name.Should().Be(name);
        medicine.BoxPrice.Should().Be(boxPrice);
        medicine.StripsPerBox.Should().Be(stripsPerBox);
        medicine.StripPrice.Should().Be(10m);
        medicine.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AddStock_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var medicine = CreateValidMedicine();
        var initialStock = medicine.TotalStripsInStock;

        // Act
        medicine.AddStock(50);

        // Assert
        medicine.TotalStripsInStock.Should().Be(initialStock + 50);
    }

    [Fact]
    public void AddStock_ToDiscontinuedMedicine_ShouldThrowException()
    {
        // Arrange
        var medicine = CreateValidMedicine();
        medicine.Discontinue();

        // Act
        var act = () => medicine.AddStock(50);

        // Assert
        act.Should().Throw<DiscontinuedMedicineException>();
    }

    [Fact]
    public void RemoveStock_WithValidQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var medicine = CreateValidMedicine(initialStock: 100);

        // Act
        medicine.RemoveStock(30);

        // Assert
        medicine.TotalStripsInStock.Should().Be(70);
    }

    [Fact]
    public void RemoveStock_ExceedingAvailable_ShouldThrowException()
    {
        // Arrange
        var medicine = CreateValidMedicine(initialStock: 50);

        // Act
        var act = () => medicine.RemoveStock(100);

        // Assert
        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void StockStatus_ShouldReflectCurrentStock()
    {
        // Arrange & Act
        var outOfStock = CreateValidMedicine(initialStock: 0);
        var lowStock = CreateValidMedicine(initialStock: 5, minimumStockLevel: 10);
        var needsReorder = CreateValidMedicine(initialStock: 15, minimumStockLevel: 10, reorderLevel: 20);
        var inStock = CreateValidMedicine(initialStock: 100);

        // Assert
        outOfStock.StockStatus.Should().Be(StockStatus.OutOfStock);
        lowStock.StockStatus.Should().Be(StockStatus.LowStock);
        needsReorder.StockStatus.Should().Be(StockStatus.NeedsReorder);
        inStock.StockStatus.Should().Be(StockStatus.InStock);
    }

    [Fact]
    public void IsExpiringSoon_WithExpiryIn30Days_ShouldReturnTrue()
    {
        // Arrange
        var medicine = CreateValidMedicine(expiryDate: DateTime.UtcNow.AddDays(15));

        // Act & Assert
        medicine.IsExpiringSoon.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalseForFutureDate()
    {
        // Arrange
        var medicine = CreateValidMedicine(expiryDate: DateTime.UtcNow.AddDays(60));

        // Act & Assert
        medicine.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void CalculatePrice_ForStrips_ShouldReturnCorrectPrice()
    {
        // Arrange
        var medicine = CreateValidMedicine(); // BoxPrice = 100, StripsPerBox = 10

        // Act
        var price = medicine.CalculatePrice(5);

        // Assert
        price.Should().Be(50m); // 5 strips * 10 per strip
    }

    [Fact]
    public void CalculatePrice_ForBoxesAndStrips_ShouldReturnCorrectPrice()
    {
        // Arrange
        var medicine = CreateValidMedicine(); // BoxPrice = 100, StripsPerBox = 10

        // Act
        var price = medicine.CalculatePrice(boxes: 2, strips: 5);

        // Assert
        price.Should().Be(250m); // (2 * 100) + (5 * 10)
    }

    [Fact]
    public void FullBoxesInStock_ShouldCalculateCorrectly()
    {
        // Arrange
        var medicine = CreateValidMedicine(initialStock: 25); // 10 strips per box

        // Act & Assert
        medicine.FullBoxesInStock.Should().Be(2);
        medicine.RemainingStrips.Should().Be(5);
    }

    [Fact]
    public void InventoryValue_ShouldCalculateCorrectly()
    {
        // Arrange
        var medicine = CreateValidMedicine(initialStock: 25); // BoxPrice = 100, StripsPerBox = 10

        // Act
        var value = medicine.InventoryValue;

        // Assert
        // 2 full boxes (200) + 5 strips (50) = 250
        value.Should().Be(250m);
    }

    [Fact]
    public void Discontinue_ShouldMarkAsInactive()
    {
        // Arrange
        var medicine = CreateValidMedicine();

        // Act
        medicine.Discontinue();

        // Assert
        medicine.IsDiscontinued.Should().BeTrue();
        medicine.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_ShouldMarkAsActive()
    {
        // Arrange
        var medicine = CreateValidMedicine();
        medicine.Discontinue();

        // Act
        medicine.Reactivate();

        // Assert
        medicine.IsDiscontinued.Should().BeFalse();
        medicine.IsActive.Should().BeTrue();
    }

    private static Medicine CreateValidMedicine(
        int initialStock = 0,
        int minimumStockLevel = 10,
        int reorderLevel = 20,
        DateTime? expiryDate = null)
    {
        return Medicine.Create(
            Guid.NewGuid(),
            "Paracetamol",
            boxPrice: 100m,
            stripsPerBox: 10,
            initialStock: initialStock,
            minimumStockLevel: minimumStockLevel,
            reorderLevel: reorderLevel,
            expiryDate: expiryDate);
    }
}
