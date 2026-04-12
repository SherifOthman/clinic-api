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
    public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";

    // Not Found (404)
    public const string RESOURCE_NOT_FOUND = "RESOURCE_NOT_FOUND";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string PATIENT_NOT_FOUND = "PATIENT_NOT_FOUND";
    public const string DOCTOR_NOT_FOUND = "DOCTOR_NOT_FOUND";
    public const string BRANCH_NOT_FOUND = "BRANCH_NOT_FOUND";
    public const string APPOINTMENT_NOT_FOUND = "APPOINTMENT_NOT_FOUND";
    public const string DISEASE_NOT_FOUND = "DISEASE_NOT_FOUND";
    public const string PLAN_NOT_FOUND = "PLAN_NOT_FOUND";
    public const string CLINIC_NOT_FOUND = "CLINIC_NOT_FOUND";
    public const string NOT_FOUND = "NOT_FOUND";

    // Duplicates
    public const string DUPLICATE_ENTRY = "DUPLICATE_ENTRY";
    public const string DISEASE_ALREADY_EXISTS = "DISEASE_ALREADY_EXISTS";
    public const string DUPLICATE_SERVICE = "DUPLICATE_SERVICE";
    public const string DUPLICATE_SUPPLY = "DUPLICATE_SUPPLY";
    public const string DUPLICATE_ATTRIBUTE = "DUPLICATE_ATTRIBUTE";
    public const string ALREADY_EXISTS = "ALREADY_EXISTS";
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string USERNAME_ALREADY_EXISTS = "USERNAME_ALREADY_EXISTS";

    // Appointments
    public const string APPOINTMENT_CONFLICT = "APPOINTMENT_CONFLICT";
    public const string APPOINTMENT_PAST_DATE = "APPOINTMENT_PAST_DATE";
    public const string APPOINTMENT_OUTSIDE_HOURS = "APPOINTMENT_OUTSIDE_HOURS";
    public const string APPOINTMENT_ALREADY_CONFIRMED = "APPOINTMENT_ALREADY_CONFIRMED";
    public const string APPOINTMENT_ALREADY_COMPLETED = "APPOINTMENT_ALREADY_COMPLETED";
    public const string APPOINTMENT_ALREADY_CANCELLED = "APPOINTMENT_ALREADY_CANCELLED";
    public const string CANNOT_CANCEL_APPOINTMENT = "CANNOT_CANCEL_APPOINTMENT";
    public const string CANNOT_CONFIRM_APPOINTMENT = "CANNOT_CONFIRM_APPOINTMENT";

    // Billing
    public const string INVOICE_NOT_FOUND = "INVOICE_NOT_FOUND";
    public const string INVOICE_ALREADY_PAID = "INVOICE_ALREADY_PAID";
    public const string INVOICE_ALREADY_CANCELLED = "INVOICE_ALREADY_CANCELLED";
    public const string INVOICE_NOT_PAYABLE = "INVOICE_NOT_PAYABLE";
    public const string PAYMENT_EXCEEDS_BALANCE = "PAYMENT_EXCEEDS_BALANCE";
    public const string INVALID_PAYMENT_AMOUNT = "INVALID_PAYMENT_AMOUNT";
    public const string INVALID_DISCOUNT = "INVALID_DISCOUNT";
    public const string DISCOUNT_EXCEEDS_TOTAL = "DISCOUNT_EXCEEDS_TOTAL";

    // Inventory / Medicines
    public const string MEDICINE_NOT_FOUND = "MEDICINE_NOT_FOUND";
    public const string MEDICINE_EXPIRED = "MEDICINE_EXPIRED";
    public const string MEDICINE_DISCONTINUED = "MEDICINE_DISCONTINUED";
    public const string MEDICINE_OUT_OF_STOCK = "MEDICINE_OUT_OF_STOCK";
    public const string INSUFFICIENT_STOCK = "INSUFFICIENT_STOCK";
    public const string INVALID_STOCK_QUANTITY = "INVALID_STOCK_QUANTITY";
    public const string NEGATIVE_STOCK = "NEGATIVE_STOCK";

    // Medical / Prescriptions
    public const string PATIENT_HAS_ALLERGY = "PATIENT_HAS_ALLERGY";
    public const string CRITICAL_ALLERGY_WARNING = "CRITICAL_ALLERGY_WARNING";
    public const string MEDICATION_INTERACTION = "MEDICATION_INTERACTION";

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

    // System (500)
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
}
