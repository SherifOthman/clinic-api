namespace ClinicManagement.Application.Common.Constants;

public static class ApplicationErrors
{
    public static class Authentication
    {
        public const string USER_NOT_AUTHENTICATED = "User not authenticated";
        public const string USER_NOT_FOUND = "User not found";
        public const string INVALID_CREDENTIALS = "Invalid email or password";
        public const string EMAIL_NOT_CONFIRMED = "Email address is not confirmed";
        public const string INVALID_PASSWORD = "Current password is incorrect";
        public const string PASSWORD_MISMATCH = "Passwords do not match";
        public const string INVALID_RESET_TOKEN = "Invalid reset token";
        public const string EMAIL_ALREADY_CONFIRMED = "Email is already confirmed";
        public const string AUTHENTICATION_FAILED = "Authentication failed";

        public static string UserWithEmailNotFound(string email) => $"User with email '{email}' not found";
    }

    public static class Authorization
    {
        public const string INSUFFICIENT_PERMISSIONS = "Insufficient permissions to perform this action";
        public const string ACCESS_DENIED = "Access denied";
        public const string USER_NO_CLINIC_ACCESS = "User does not have clinic access";
        public const string USER_CLINIC_NOT_FOUND = "User clinic not found";
    }

    public static class Validation
    {
        public const string REQUIRED_FIELD = "Field is required";
        public const string INVALID_FORMAT = "Field has invalid format";
        public const string INVALID_LENGTH = "Field length is invalid";
        public const string INVALID_EMAIL = "Invalid email address";
        public const string INVALID_PHONE = "Invalid phone number";
        public const string EMAIL_ALREADY_REGISTERED = "This email address is already registered.";
        public const string USERNAME_ALREADY_TAKEN = "This username is already taken.";

        public static string RequiredField(string fieldName) => $"{fieldName} is required";
        public static string InvalidFormat(string fieldName) => $"{fieldName} has invalid format";
        public static string InvalidLength(string fieldName, int min, int max) => $"{fieldName} must be between {min} and {max} characters";
    }

    public static class Business
    {
        public const string ENTITY_NOT_FOUND = "Entity not found";
        public const string ENTITY_ALREADY_EXISTS = "Entity already exists";
        public const string OPERATION_NOT_ALLOWED = "Operation is not allowed";
        public const string INVALID_OPERATION = "Operation failed";
        public const string PATIENT_NOT_FOUND = "Patient not found";
        public const string SUBSCRIPTION_PLAN_NOT_FOUND = "Subscription plan not found";
        public const string CHRONIC_DISEASE_NOT_FOUND = "Chronic disease not found";

        public static string EntityNotFound(string entityName) => $"{entityName} not found";
        public static string EntityAlreadyExists(string entityName) => $"{entityName} already exists";
        public static string OperationNotAllowed(string operation) => $"{operation} is not allowed";
    }

    public static class File
    {
        public const string FILE_NOT_FOUND = "File not found";
        public const string FILE_TOO_LARGE = "File size exceeds maximum allowed size";
        public const string INVALID_FILE_TYPE = "Invalid file type";
        public const string FILE_UPLOAD_FAILED = "File upload failed";
        public const string FILE_DELETE_FAILED = "Failed to delete file";
    }

    public static class Onboarding
    {
        public const string USER_ALREADY_HAS_CLINIC = "User already has a clinic";
        public const string INVALID_SUBSCRIPTION_PLAN = "Invalid subscription plan";
        public const string ONBOARDING_INCOMPLETE = "Onboarding is not complete";
    }

    public static class Appointment
    {
        public const string APPOINTMENT_DATE_IN_PAST = "Appointment date cannot be in the past";
        public const string APPOINTMENT_NOT_FOUND = "Appointment not found";
        public const string APPOINTMENT_ALREADY_CONFIRMED = "Appointment is already confirmed";
        public const string APPOINTMENT_CANNOT_BE_CANCELLED = "Appointment cannot be cancelled";
        public const string INVALID_APPOINTMENT_TIME = "Invalid appointment time";
        public const string DOCTOR_NOT_AVAILABLE = "Doctor is not available at the selected time";
    }
}