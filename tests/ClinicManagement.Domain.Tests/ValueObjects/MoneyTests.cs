using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.ValueObjects;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Theory]
    [InlineData(100.50, "USD")]
    [InlineData(0, "EUR")]
    [InlineData(-50.25, "GBP")]
    public void Constructor_ValidMoney_ShouldCreateMoney(decimal amount, string currency)
    {
        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Amount.Should().Be(Math.Round(amount, 2));
        money.Currency.Should().Be(currency.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_EmptyCurrency_ShouldThrowException(string currency)
    {
        // Act
        Action act = () => new Money(100, currency);

        // Assert
        act.Should().Throw<InvalidMoneyException>()
            .WithMessage("Currency cannot be empty");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("1")]
    public void Constructor_InvalidCurrencyLength_ShouldThrowException(string currency)
    {
        // Act
        Action act = () => new Money(100, currency);

        // Assert
        act.Should().Throw<InvalidMoneyException>()
            .WithMessage("Currency must be 3 characters (ISO 4217)");
    }

    [Fact]
    public void Constructor_ShouldRoundToTwoDecimalPlaces()
    {
        // Arrange
        var amount = 100.12345m;

        // Act
        var money = new Money(amount, "USD");

        // Assert
        money.Amount.Should().Be(100.12m);
    }

    [Fact]
    public void Zero_ShouldCreateZeroMoney()
    {
        // Act
        var money = Money.Zero("USD");

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("USD");
        money.IsZero.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(100, false)]
    [InlineData(-100, false)]
    public void IsZero_ShouldReturnCorrectValue(decimal amount, bool expected)
    {
        // Arrange
        var money = new Money(amount, "USD");

        // Act & Assert
        money.IsZero.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-100, false)]
    public void IsPositive_ShouldReturnCorrectValue(decimal amount, bool expected)
    {
        // Arrange
        var money = new Money(amount, "USD");

        // Act & Assert
        money.IsPositive.Should().Be(expected);
    }

    [Theory]
    [InlineData(-100, true)]
    [InlineData(0, false)]
    [InlineData(100, false)]
    public void IsNegative_ShouldReturnCorrectValue(decimal amount, bool expected)
    {
        // Arrange
        var money = new Money(amount, "USD");

        // Act & Assert
        money.IsNegative.Should().Be(expected);
    }

    [Fact]
    public void Add_SameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(50, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(50, "EUR");

        // Act
        Action act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidMoneyException>()
            .WithMessage("Cannot add different currencies: USD and EUR");
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldSubtractAmounts()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(30, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_ShouldMultiplyAmount()
    {
        // Arrange
        var money = new Money(100, "USD");

        // Act
        var result = money.Multiply(2.5m);

        // Assert
        result.Amount.Should().Be(250);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Divide_ShouldDivideAmount()
    {
        // Arrange
        var money = new Money(100, "USD");

        // Act
        var result = money.Divide(4);

        // Assert
        result.Amount.Should().Be(25);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Divide_ByZero_ShouldThrowException()
    {
        // Arrange
        var money = new Money(100, "USD");

        // Act
        Action act = () => money.Divide(0);

        // Assert
        act.Should().Throw<InvalidMoneyException>()
            .WithMessage("Cannot divide by zero");
    }

    [Fact]
    public void Percentage_ShouldCalculatePercentage()
    {
        // Arrange
        var money = new Money(100, "USD");

        // Act
        var result = money.Percentage(15);

        // Assert
        result.Amount.Should().Be(15);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Abs_ShouldReturnAbsoluteValue()
    {
        // Arrange
        var money = new Money(-100, "USD");

        // Act
        var result = money.Abs();

        // Assert
        result.Amount.Should().Be(100);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Negate_ShouldNegateAmount()
    {
        // Arrange
        var money = new Money(100, "USD");

        // Act
        var result = money.Negate();

        // Assert
        result.Amount.Should().Be(-100);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void OperatorPlus_ShouldAddMoney()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(50, "USD");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150);
    }

    [Fact]
    public void OperatorMinus_ShouldSubtractMoney()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(30, "USD");

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(70);
    }

    [Fact]
    public void OperatorMultiply_ShouldMultiplyMoney()
    {
        // Arrange
        var money = new Money(100, "USD");

        // Act
        var result = money * 2;

        // Assert
        result.Amount.Should().Be(200);
    }

    [Fact]
    public void OperatorDivide_ShouldDivideMoney()
    {
        // Arrange
        var money = new Money(100, "USD");

        // Act
        var result = money / 4;

        // Assert
        result.Amount.Should().Be(25);
    }

    [Fact]
    public void OperatorNegate_ShouldNegateMoney()
    {
        // Arrange
        var money = new Money(100, "USD");

        // Act
        var result = -money;

        // Assert
        result.Amount.Should().Be(-100);
    }

    [Fact]
    public void OperatorGreaterThan_ShouldCompareCorrectly()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(50, "USD");

        // Act & Assert
        (money1 > money2).Should().BeTrue();
        (money2 > money1).Should().BeFalse();
    }

    [Fact]
    public void OperatorLessThan_ShouldCompareCorrectly()
    {
        // Arrange
        var money1 = new Money(50, "USD");
        var money2 = new Money(100, "USD");

        // Act & Assert
        (money1 < money2).Should().BeTrue();
        (money2 < money1).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameValue_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(100, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentAmount_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(50, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCurrency_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(100, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var money = new Money(1234.56m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("1,234.56 USD");
    }
}
