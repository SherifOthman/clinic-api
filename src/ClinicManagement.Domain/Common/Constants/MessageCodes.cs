namespace ClinicManagement.Domain.Common.Constants;

/// <summary>
/// Simplified message codes for error handling
/// 
/// APPROACH:
/// - ~30 codes for business logic errors that need specific UI handling
/// - Validation errors don't need codes (use Result.FailValidation())
/// - System errors use generic codes (NOT_FOUND, INTERNAL_ERROR, etc.)
/// 
/// USAGE:
/// - Use Auth.* for authentication/authorization errors
/// - Use Business.* for domain business rules
/// - Use System.* for technical/system errors
/// - Don't use codes for validation errors (field-level)
/// </summary>
public static class MessageCodes
{
    /// <summary>
    /// Authentication and authorization errors
    /// These need specific UI handling (redirect to login, show confirmation prompt, etc.)
    /// </summary>
    public static class Auth
    {
        public const string INVALID_CREDENTIALS = "AUTH.INVALID_CREDENTIALS";
        public const string EMAIL_NOT_CONFIRMED = "AUTH.EMAIL_NOT_CONFIRMED";
        public const string UNAUTHORIZED = "AUTH.UNAUTHORIZED";
        public const string SESSION_EXPIRED = "AUTH.SESSION_EXPIRED";
        public const string USER_NOT_FOUND = "AUTH.USER_NOT_FOUND";
        public const string INVALID_TOKEN = "AUTH.INVALID_TOKEN";
        public const string ACCESS_DENIED = "AUTH.ACCESS_DENIED";
    }

    /// <summary>
    /// Business logic errors
    /// These represent domain rules that need specific UI handling
    /// </summary>
    public static class Business
    {
        // Inventory & Stock
        public const string INSUFFICIENT_STOCK = "BUSINESS.INSUFFICIENT_STOCK";
        public const string MEDICINE_EXPIRED = "BUSINESS.MEDICINE_EXPIRED";
        public const string MEDICINE_DISCONTINUED = "BUSINESS.MEDICINE_DISCONTINUED";
        public const string LOW_STOCK_WARNING = "BUSINESS.LOW_STOCK_WARNING";

        // Appointments
        public const string APPOINTMENT_CONFLICT = "BUSINESS.APPOINTMENT_CONFLICT";
        public const string APPOINTMENT_PAST_DATE = "BUSINESS.APPOINTMENT_PAST_DATE";
        public const string APPOINTMENT_ALREADY_COMPLETED = "BUSINESS.APPOINTMENT_ALREADY_COMPLETED";
        public const string APPOINTMENT_CANCELLED = "BUSINESS.APPOINTMENT_CANCELLED";

        // Billing & Payments
        public const string INVOICE_ALREADY_PAID = "BUSINESS.INVOICE_ALREADY_PAID";
        public const string INVOICE_CANCELLED = "BUSINESS.INVOICE_CANCELLED";
        public const string PAYMENT_EXCEEDS_AMOUNT = "BUSINESS.PAYMENT_EXCEEDS_AMOUNT";
        public const string INVALID_DISCOUNT = "BUSINESS.INVALID_DISCOUNT";

        // Patient Safety
        public const string PATIENT_HAS_ALLERGY = "BUSINESS.PATIENT_HAS_ALLERGY";
        public const string CRITICAL_ALLERGY_WARNING = "BUSINESS.CRITICAL_ALLERGY_WARNING";

        // Invitations
        public const string INVITATION_EXPIRED = "BUSINESS.INVITATION_EXPIRED";
        public const string INVITATION_ALREADY_ACCEPTED = "BUSINESS.INVITATION_ALREADY_ACCEPTED";
        public const string INVITATION_INVALID = "BUSINESS.INVITATION_INVALID";

        // General Business Rules
        public const string DUPLICATE_ENTRY = "BUSINESS.DUPLICATE_ENTRY";
        public const string OPERATION_NOT_ALLOWED = "BUSINESS.OPERATION_NOT_ALLOWED";
        public const string INVALID_STATE_TRANSITION = "BUSINESS.INVALID_STATE_TRANSITION";
        public const string ENTITY_NOT_FOUND = "BUSINESS.ENTITY_NOT_FOUND";
        public const string CHRONIC_DISEASE_NOT_FOUND = "BUSINESS.CHRONIC_DISEASE_NOT_FOUND";
        public const string CHRONIC_DISEASE_ALREADY_EXISTS = "BUSINESS.CHRONIC_DISEASE_ALREADY_EXISTS";
    }

    /// <summary>
    /// System-level errors
    /// These indicate technical issues
    /// </summary>
    public static class System
    {
        public const string NOT_FOUND = "SYSTEM.NOT_FOUND";
        public const string INTERNAL_ERROR = "SYSTEM.INTERNAL_ERROR";
        public const string SERVICE_UNAVAILABLE = "SYSTEM.SERVICE_UNAVAILABLE";
        public const string DATABASE_ERROR = "SYSTEM.DATABASE_ERROR";
        public const string EXTERNAL_SERVICE_ERROR = "SYSTEM.EXTERNAL_SERVICE_ERROR";
        public const string UNAUTHENTICATED = "SYSTEM.UNAUTHENTICATED";
    }
}
