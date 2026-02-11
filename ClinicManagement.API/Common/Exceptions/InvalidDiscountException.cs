namespace ClinicManagement.API.Common.Exceptions;

/// <summary>
/// Exception thrown when a discount is invalid
/// </summary>
public class InvalidDiscountException : DomainException
{
    public decimal DiscountAmount { get; }
    public decimal SubtotalAmount { get; }

    public InvalidDiscountException(decimal discountAmount, decimal subtotalAmount, string? errorCode = null) 
        : base($"Invalid discount. Discount: {discountAmount:C}, Subtotal: {subtotalAmount:C}", errorCode ?? string.Empty)
    {
        DiscountAmount = discountAmount;
        SubtotalAmount = subtotalAmount;
    }
}
