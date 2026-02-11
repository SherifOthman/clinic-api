namespace ClinicManagement.API.Common.Exceptions;

public class InvalidDiscountException : DomainException
{
    public decimal DiscountAmount { get; }
    public decimal SubtotalAmount { get; }

    public InvalidDiscountException(decimal discountAmount, decimal subtotalAmount) 
        : base("INVALID_DISCOUNT", $"Invalid discount. Discount: {discountAmount:C}, Subtotal: {subtotalAmount:C}")
    {
        DiscountAmount = discountAmount;
        SubtotalAmount = subtotalAmount;
    }
}
