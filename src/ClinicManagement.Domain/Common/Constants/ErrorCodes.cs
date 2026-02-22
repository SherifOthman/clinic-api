namespace ClinicManagement.Domain.Common.Constants;

/// <summary>
/// Standard error codes for frontend i18n translation
/// Frontend maps these to localized messages in Arabic/English
/// </summary>
public static class ErrorCodes
{
    // Validation (400)
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string REQUIRED_FIELD = "REQUIRED_FIELD";
    public const string INVALID_FORMAT = "INVALID_FORMAT";

    // Authentication & Authorization (401, 403)
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string EMAIL_NOT_CONFIRMED = "EMAIL_NOT_CONFIRMED";
    public const string EMAIL_ALREADY_CONFIRMED = "EMAIL_ALREADY_CONFIRMED";
    public const string TOKEN_INVALID = "TOKEN_INVALID";
    public const string ACCESS_DENIED = "ACCESS_DENIED";
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string FORBIDDEN = "FORBIDDEN";

    // Not Found (404)
    public const string RESOURCE_NOT_FOUND = "RESOURCE_NOT_FOUND";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string PATIENT_NOT_FOUND = "PATIENT_NOT_FOUND";
    public const string DOCTOR_NOT_FOUND = "DOCTOR_NOT_FOUND";
    public const string BRANCH_NOT_FOUND = "BRANCH_NOT_FOUND";
    public const string APPOINTMENT_NOT_FOUND = "APPOINTMENT_NOT_FOUND";
    public const string DISEASE_NOT_FOUND = "DISEASE_NOT_FOUND";
    public const string PLAN_NOT_FOUND = "PLAN_NOT_FOUND";
    public const string NOT_FOUND = "NOT_FOUND";

    // Duplicates
    public const string DUPLICATE_ENTRY = "DUPLICATE_ENTRY";
    public const string DISEASE_ALREADY_EXISTS = "DISEASE_ALREADY_EXISTS";
    public const string DUPLICATE_SERVICE = "DUPLICATE_SERVICE";
    public const string DUPLICATE_SUPPLY = "DUPLICATE_SUPPLY";
    public const string DUPLICATE_ATTRIBUTE = "DUPLICATE_ATTRIBUTE";

    // Appointments
    public const string APPOINTMENT_CONFLICT = "APPOINTMENT_CONFLICT";

    // Onboarding
    public const string ALREADY_ONBOARDED = "ALREADY_ONBOARDED";

    // Staff Invitations
    public const string INVITATION_ALREADY_ACCEPTED = "INVITATION_ALREADY_ACCEPTED";
    public const string INVITATION_ALREADY_CANCELED = "INVITATION_ALREADY_CANCELED";
    public const string INVITATION_EXPIRED = "INVITATION_EXPIRED";

    // General
    public const string OPERATION_NOT_ALLOWED = "OPERATION_NOT_ALLOWED";
    public const string OPERATION_FAILED = "OPERATION_FAILED";

    // System (500)
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
}
