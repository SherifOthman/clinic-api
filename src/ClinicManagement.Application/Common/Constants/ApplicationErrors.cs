namespace ClinicManagement.Application.Common.Constants;

/// <summary>
/// Application error codes for internationalization.
/// Use MessageCodes class for the actual message codes.
/// This class is kept for backward compatibility and will be deprecated.
/// </summary>
[Obsolete("Use MessageCodes class instead for internationalization support")]
public static class ApplicationErrors
{
    public static class Authentication
    {
        public const string USER_NOT_AUTHENTICATED = MessageCodes.Authentication.USER_NOT_AUTHENTICATED;
        public const string USER_NOT_FOUND = MessageCodes.Authentication.USER_NOT_FOUND;
        public const string INVALID_CREDENTIALS = MessageCodes.Authentication.INVALID_CREDENTIALS;
        public const string EMAIL_NOT_CONFIRMED = MessageCodes.Authentication.EMAIL_NOT_CONFIRMED;
        public const string INVALID_PASSWORD = MessageCodes.Authentication.INVALID_PASSWORD;
        public const string PASSWORD_MISMATCH = MessageCodes.Authentication.PASSWORD_MISMATCH;
        public const string INVALID_RESET_TOKEN = MessageCodes.Authentication.INVALID_RESET_TOKEN;
        public const string EMAIL_ALREADY_CONFIRMED = MessageCodes.Authentication.EMAIL_ALREADY_CONFIRMED;
        public const string AUTHENTICATION_FAILED = MessageCodes.Authentication.AUTHENTICATION_FAILED;

        public static string UserWithEmailNotFound(string email) => MessageCodes.Authentication.USER_WITH_EMAIL_NOT_FOUND;
    }

    public static class Authorization
    {
        public const string INSUFFICIENT_PERMISSIONS = MessageCodes.Authorization.INSUFFICIENT_PERMISSIONS;
        public const string ACCESS_DENIED = MessageCodes.Authorization.ACCESS_DENIED;
        public const string USER_NO_CLINIC_ACCESS = MessageCodes.Authorization.USER_NO_CLINIC_ACCESS;
        public const string USER_CLINIC_NOT_FOUND = MessageCodes.Authorization.USER_CLINIC_NOT_FOUND;
    }

    public static class Validation
    {
        public const string REQUIRED_FIELD = MessageCodes.Validation.REQUIRED_FIELD;
        public const string INVALID_FORMAT = MessageCodes.Validation.INVALID_FORMAT;
        public const string INVALID_LENGTH = MessageCodes.Validation.INVALID_LENGTH;
        public const string INVALID_EMAIL = MessageCodes.Validation.INVALID_EMAIL;
        public const string INVALID_PHONE = MessageCodes.Validation.INVALID_PHONE;
        public const string EMAIL_ALREADY_REGISTERED = MessageCodes.Validation.EMAIL_ALREADY_REGISTERED;
        public const string USERNAME_ALREADY_TAKEN = MessageCodes.Validation.USERNAME_ALREADY_TAKEN;

        public static string RequiredField(string fieldName) => MessageCodes.Validation.FIELD_REQUIRED;
        public static string InvalidFormat(string fieldName) => MessageCodes.Validation.FIELD_INVALID_FORMAT;
        public static string InvalidLength(string fieldName, int min, int max) => MessageCodes.Validation.FIELD_INVALID_LENGTH;
    }

    public static class Business
    {
        public const string ENTITY_NOT_FOUND = MessageCodes.Business.ENTITY_NOT_FOUND;
        public const string ENTITY_ALREADY_EXISTS = MessageCodes.Business.ENTITY_ALREADY_EXISTS;
        public const string OPERATION_NOT_ALLOWED = MessageCodes.Business.OPERATION_NOT_ALLOWED;
        public const string INVALID_OPERATION = MessageCodes.Business.INVALID_OPERATION;
        public const string PATIENT_NOT_FOUND = MessageCodes.Business.PATIENT_NOT_FOUND;
        public const string SUBSCRIPTION_PLAN_NOT_FOUND = MessageCodes.Business.SUBSCRIPTION_PLAN_NOT_FOUND;
        public const string CHRONIC_DISEASE_NOT_FOUND = MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND;

        public static string EntityNotFound(string entityName) => MessageCodes.Business.ENTITY_NOT_FOUND;
        public static string EntityAlreadyExists(string entityName) => MessageCodes.Business.ENTITY_ALREADY_EXISTS;
        public static string OperationNotAllowed(string operation) => MessageCodes.Business.OPERATION_NOT_ALLOWED;
    }

    public static class File
    {
        public const string FILE_NOT_FOUND = MessageCodes.File.FILE_NOT_FOUND;
        public const string FILE_TOO_LARGE = MessageCodes.File.FILE_TOO_LARGE;
        public const string INVALID_FILE_TYPE = MessageCodes.File.INVALID_FILE_TYPE;
        public const string FILE_UPLOAD_FAILED = MessageCodes.File.FILE_UPLOAD_FAILED;
        public const string FILE_DELETE_FAILED = MessageCodes.File.FILE_DELETE_FAILED;
    }

    public static class Onboarding
    {
        public const string USER_ALREADY_HAS_CLINIC = MessageCodes.Onboarding.USER_ALREADY_HAS_CLINIC;
        public const string INVALID_SUBSCRIPTION_PLAN = MessageCodes.Onboarding.INVALID_SUBSCRIPTION_PLAN;
        public const string ONBOARDING_INCOMPLETE = MessageCodes.Onboarding.ONBOARDING_INCOMPLETE;
    }

    public static class Appointment
    {
        public const string APPOINTMENT_DATE_IN_PAST = MessageCodes.Appointment.APPOINTMENT_DATE_IN_PAST;
        public const string APPOINTMENT_NOT_FOUND = MessageCodes.Appointment.APPOINTMENT_NOT_FOUND;
        public const string APPOINTMENT_ALREADY_CONFIRMED = MessageCodes.Appointment.APPOINTMENT_ALREADY_CONFIRMED;
        public const string APPOINTMENT_CANNOT_BE_CANCELLED = MessageCodes.Appointment.APPOINTMENT_CANNOT_BE_CANCELLED;
        public const string INVALID_APPOINTMENT_TIME = MessageCodes.Appointment.INVALID_APPOINTMENT_TIME;
        public const string DOCTOR_NOT_AVAILABLE = MessageCodes.Appointment.DOCTOR_NOT_AVAILABLE;
    }
}