namespace ClinicManagement.API.Common.Constants;

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
    public const string INVALID_EMAIL = "INVALID_EMAIL";
    public const string INVALID_PHONE = "INVALID_PHONE";
    public const string INVALID_DATE = "INVALID_DATE";
    public const string INVALID_RANGE = "INVALID_RANGE";
    public const string TOO_SHORT = "TOO_SHORT";
    public const string TOO_LONG = "TOO_LONG";

    // Authentication & Authorization (401, 403)
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string EMAIL_NOT_CONFIRMED = "EMAIL_NOT_CONFIRMED";
    public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
    public const string ACCOUNT_DISABLED = "ACCOUNT_DISABLED";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
    public const string TOKEN_INVALID = "TOKEN_INVALID";
    public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";
    public const string ACCESS_DENIED = "ACCESS_DENIED";

    // Uniqueness / Duplicate (409)
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string USERNAME_ALREADY_EXISTS = "USERNAME_ALREADY_EXISTS";
    public const string PHONE_ALREADY_EXISTS = "PHONE_ALREADY_EXISTS";
    public const string DUPLICATE_ENTRY = "DUPLICATE_ENTRY";

    // Not Found (404)
    public const string RESOURCE_NOT_FOUND = "RESOURCE_NOT_FOUND";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string PATIENT_NOT_FOUND = "PATIENT_NOT_FOUND";
    public const string APPOINTMENT_NOT_FOUND = "APPOINTMENT_NOT_FOUND";
    public const string INVOICE_NOT_FOUND = "INVOICE_NOT_FOUND";
    public const string MEDICINE_NOT_FOUND = "MEDICINE_NOT_FOUND";

    // Appointments
    public const string APPOINTMENT_CONFLICT = "APPOINTMENT_CONFLICT";
    public const string APPOINTMENT_PAST_DATE = "APPOINTMENT_PAST_DATE";
    public const string APPOINTMENT_OUTSIDE_HOURS = "APPOINTMENT_OUTSIDE_HOURS";
    public const string APPOINTMENT_ALREADY_CONFIRMED = "APPOINTMENT_ALREADY_CONFIRMED";
    public const string APPOINTMENT_ALREADY_COMPLETED = "APPOINTMENT_ALREADY_COMPLETED";
    public const string APPOINTMENT_ALREADY_CANCELLED = "APPOINTMENT_ALREADY_CANCELLED";
    public const string CANNOT_CANCEL_APPOINTMENT = "CANNOT_CANCEL_APPOINTMENT";
    public const string CANNOT_CONFIRM_APPOINTMENT = "CANNOT_CONFIRM_APPOINTMENT";

    // Inventory & Stock
    public const string INSUFFICIENT_STOCK = "INSUFFICIENT_STOCK";
    public const string MEDICINE_EXPIRED = "MEDICINE_EXPIRED";
    public const string MEDICINE_DISCONTINUED = "MEDICINE_DISCONTINUED";
    public const string MEDICINE_OUT_OF_STOCK = "MEDICINE_OUT_OF_STOCK";
    public const string INVALID_STOCK_QUANTITY = "INVALID_STOCK_QUANTITY";
    public const string NEGATIVE_STOCK = "NEGATIVE_STOCK";

    // Billing & Payments
    public const string INVOICE_ALREADY_PAID = "INVOICE_ALREADY_PAID";
    public const string INVOICE_ALREADY_CANCELLED = "INVOICE_ALREADY_CANCELLED";
    public const string INVOICE_NOT_PAYABLE = "INVOICE_NOT_PAYABLE";
    public const string PAYMENT_EXCEEDS_BALANCE = "PAYMENT_EXCEEDS_BALANCE";
    public const string INVALID_PAYMENT_AMOUNT = "INVALID_PAYMENT_AMOUNT";
    public const string INVALID_DISCOUNT = "INVALID_DISCOUNT";
    public const string DISCOUNT_EXCEEDS_TOTAL = "DISCOUNT_EXCEEDS_TOTAL";

    // Patient Safety
    public const string PATIENT_HAS_ALLERGY = "PATIENT_HAS_ALLERGY";
    public const string CRITICAL_ALLERGY_WARNING = "CRITICAL_ALLERGY_WARNING";
    public const string MEDICATION_INTERACTION = "MEDICATION_INTERACTION";

    // Invitations
    public const string INVITATION_EXPIRED = "INVITATION_EXPIRED";
    public const string INVITATION_ALREADY_ACCEPTED = "INVITATION_ALREADY_ACCEPTED";
    public const string INVITATION_INVALID = "INVITATION_INVALID";
    public const string INVITATION_NOT_FOUND = "INVITATION_NOT_FOUND";

    // General
    public const string OPERATION_NOT_ALLOWED = "OPERATION_NOT_ALLOWED";
    public const string INVALID_STATE_TRANSITION = "INVALID_STATE_TRANSITION";
    public const string RESOURCE_LOCKED = "RESOURCE_LOCKED";
    public const string RESOURCE_IN_USE = "RESOURCE_IN_USE";
    public const string OPERATION_FAILED = "OPERATION_FAILED";

    // System (500)
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
    public const string DATABASE_ERROR = "DATABASE_ERROR";
    public const string EXTERNAL_SERVICE_ERROR = "EXTERNAL_SERVICE_ERROR";
    public const string SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE";
}
