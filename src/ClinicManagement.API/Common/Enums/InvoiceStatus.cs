namespace ClinicManagement.API.Common.Enums;

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    PartiallyPaid = 3,
    FullyPaid = 4,
    Cancelled = 5,
    Overdue = 6
}
