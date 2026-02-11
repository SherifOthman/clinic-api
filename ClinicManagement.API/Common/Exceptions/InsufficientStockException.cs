namespace ClinicManagement.API.Common.Exceptions;

public class InsufficientStockException : DomainException
{
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(int requestedQuantity, int availableQuantity) 
        : base("INSUFFICIENT_STOCK", $"Insufficient stock. Requested: {requestedQuantity}, Available: {availableQuantity}")
    {
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
