namespace ClinicManagement.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when a discount is invalid
/// </summary>
public class InvalidDiscountException : DomainException
{
    public decimal DiscountAmount { get; }
    public decimal SubtotalAmount { get; }

    public InvalidDiscountException(decimal discountAmount, decimal subtotalAmount, string errorCode) 
        : base($"Invalid discount. Discount: {discountAmount:C}, Subtotal: {subtotalAmount:C}", errorCode)
    {
        DiscountAmount = discountAmount;
        SubtotalAmount = subtotalAmount;
    }
}
