namespace ClinicManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing a transaction's financial summary
/// </summary>
public record TransactionSummary
{
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public bool IsFullyPaid { get; init; }
    public bool IsOverpaid { get; init; }
    
    public static TransactionSummary Create(decimal totalAmount, decimal paidAmount)
    {
        var remainingAmount = totalAmount - paidAmount;
        
        return new TransactionSummary
        {
            TotalAmount = totalAmount,
            PaidAmount = paidAmount,
            RemainingAmount = remainingAmount,
            IsFullyPaid = remainingAmount <= 0,
            IsOverpaid = remainingAmount < 0
        };
    }
}