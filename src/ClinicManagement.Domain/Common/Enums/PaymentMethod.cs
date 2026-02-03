namespace ClinicManagement.Domain.Common.Enums;

public enum PaymentMethod : byte
{
    Cash = 0,
    Card = 1,
    BankTransfer = 2,
    Check = 3,
    Insurance = 4
}