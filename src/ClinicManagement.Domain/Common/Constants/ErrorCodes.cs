namespace ClinicManagement.Domain.Common.Constants;

/// <summary>
/// Standard error codes for frontend i18n translation
/// Frontend maps these to localized messages in Arabic/English
/// </summary>
public static class ErrorCodes
{
    // Validation (400)
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";

    // Authentication & Authorization (401, 403)
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string EMAIL_NOT_CONFIRMED = "EMAIL_NOT_CONFIRMED";
    public const string EMAIL_ALREADY_CONFIRMED = "EMAIL_ALREADY_CONFIRMED";
    public const string TOKEN_INVALID = "TOKEN_INVALID";
    public const string FORBIDDEN = "FORBIDDEN";
    public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";

    // Not Found (404)
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string PATIENT_NOT_FOUND = "PATIENT_NOT_FOUND";
    public const string PLAN_NOT_FOUND = "PLAN_NOT_FOUND";
    public const string CLINIC_NOT_FOUND = "CLINIC_NOT_FOUND";
    public const string NOT_FOUND = "NOT_FOUND";

    // Duplicates
    public const string ALREADY_EXISTS = "ALREADY_EXISTS";
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string USERNAME_ALREADY_EXISTS = "USERNAME_ALREADY_EXISTS";

    // Onboarding
    public const string ALREADY_ONBOARDED = "ALREADY_ONBOARDED";

    // Staff Invitations
    public const string INVITATION_ALREADY_ACCEPTED = "INVITATION_ALREADY_ACCEPTED";
    public const string INVITATION_ALREADY_CANCELED = "INVITATION_ALREADY_CANCELED";
    public const string INVITATION_CANCELED = "INVITATION_CANCELED";
    public const string INVITATION_EXPIRED = "INVITATION_EXPIRED";

    // Staff
    public const string STAFF_INACTIVE = "STAFF_INACTIVE";

    // Patients
    public const string PATIENT_NOT_DELETED = "PATIENT_NOT_DELETED";

    // File Upload
    public const string UPLOAD_FAILED = "UPLOAD_FAILED";

    // General
    public const string OPERATION_NOT_ALLOWED = "OPERATION_NOT_ALLOWED";
    public const string OPERATION_FAILED = "OPERATION_FAILED";
    public const string USER_CREATION_FAILED = "USER_CREATION_FAILED";
    public const string ROLE_ASSIGNMENT_FAILED = "ROLE_ASSIGNMENT_FAILED";
    public const string CONFLICT = "CONFLICT";

    // System (500)
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
}
