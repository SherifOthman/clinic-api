namespace ClinicManagement.Domain.Enums;

public enum AuditAction
{
    Create   = 1,
    Update   = 2,
    Delete   = 3,
    Security = 4,  // Login, Logout, FailedLogin, AccountLocked
    Restore  = 5,
}
