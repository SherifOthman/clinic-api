namespace ClinicManagement.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when there is insufficient stock for an operation
/// </summary>
public class InsufficientStockException : DomainException
{
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(int requestedQuantity, int availableQuantity, string errorCode) 
        : base($"Insufficient stock. Requested: {requestedQuantity}, Available: {availableQuantity}", errorCode)
    {
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
