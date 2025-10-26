namespace ClinicManagement.Application.Common.Constants;
public static class ErrorCodes
{
    // Message Codes
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";



    // Filed Codes
    public const string EmailRequired = "EMAIL_REQUIRED";
    public const string PasswordRequired = "PASSWORD_REQUIRED";
    public const string PasswordTooShort = "PASSWORD_TOO_SHORT";
    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string UsernameTaken = "USERNAME_TAKEN";
}
