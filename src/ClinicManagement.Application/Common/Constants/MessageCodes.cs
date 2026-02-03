namespace ClinicManagement.Application.Common.Constants;

/// <summary>
/// Centralized message codes for internationalization.
/// Format: {Category}.{Subcategory}.{MessageKey}
/// </summary>
public static class MessageCodes
{
    public static class Authentication
    {
        public const string USER_NOT_AUTHENTICATED = "AUTH.USER.NOT_AUTHENTICATED";
        public const string USER_NOT_FOUND = "AUTH.USER.NOT_FOUND";
        public const string INVALID_CREDENTIALS = "AUTH.CREDENTIALS.INVALID";
        public const string EMAIL_NOT_CONFIRMED = "AUTH.EMAIL.NOT_CONFIRMED";
        public const string INVALID_PASSWORD = "AUTH.PASSWORD.INVALID";
        public const string PASSWORD_MISMATCH = "AUTH.PASSWORD.MISMATCH";
        public const string INVALID_RESET_TOKEN = "AUTH.TOKEN.INVALID_RESET";
        public const string EMAIL_ALREADY_CONFIRMED = "AUTH.EMAIL.ALREADY_CONFIRMED";
        public const string AUTHENTICATION_FAILED = "AUTH.FAILED";
        public const string USER_WITH_EMAIL_NOT_FOUND = "AUTH.USER.EMAIL_NOT_FOUND";
        public const string UNAUTHORIZED_ACCESS = "AUTH.UNAUTHORIZED_ACCESS";
    }

    public static class Authorization
    {
        public const string INSUFFICIENT_PERMISSIONS = "AUTHZ.PERMISSIONS.INSUFFICIENT";
        public const string ACCESS_DENIED = "AUTHZ.ACCESS.DENIED";
    }

    public static class Validation
    {
        public const string REQUIRED_FIELD = "VALIDATION.FIELD.REQUIRED";
        public const string INVALID_FORMAT = "VALIDATION.FORMAT.INVALID";
        public const string INVALID_LENGTH = "VALIDATION.LENGTH.INVALID";
        public const string INVALID_EMAIL = "VALIDATION.EMAIL.INVALID";
        public const string EMAIL_ALREADY_REGISTERED = "VALIDATION.EMAIL.ALREADY_REGISTERED";
        public const string USERNAME_ALREADY_TAKEN = "VALIDATION.USERNAME.ALREADY_TAKEN";
        public const string FIELD_REQUIRED = "VALIDATION.FIELD.REQUIRED";
        public const string FIELD_INVALID_FORMAT = "VALIDATION.FIELD.INVALID_FORMAT";
        public const string FIELD_INVALID_LENGTH = "VALIDATION.FIELD.INVALID_LENGTH";
        public const string FIELD_MIN_LENGTH = "VALIDATION.FIELD.MIN_LENGTH";
        public const string FIELD_MAX_LENGTH = "VALIDATION.FIELD.MAX_LENGTH";
        public const string FIELD_INVALID_CHARACTERS = "VALIDATION.FIELD.INVALID_CHARACTERS";
        public const string PASSWORD_COMPLEXITY = "VALIDATION.PASSWORD.COMPLEXITY";
        public const string PASSWORD_NO_SPACES = "VALIDATION.PASSWORD.NO_SPACES";
        public const string PASSWORDS_MUST_MATCH = "VALIDATION.PASSWORD.MUST_MATCH";
        public const string GENERAL_VALIDATION_ERROR = "VALIDATION.GENERAL.ERROR";
    }

    public static class Business
    {
        public const string ENTITY_NOT_FOUND = "BUSINESS.ENTITY.NOT_FOUND";
        public const string ENTITY_ALREADY_EXISTS = "BUSINESS.ENTITY.ALREADY_EXISTS";
        public const string OPERATION_NOT_ALLOWED = "BUSINESS.OPERATION.NOT_ALLOWED";
        public const string INVALID_OPERATION = "BUSINESS.OPERATION.INVALID";
        public const string CHRONIC_DISEASE_NOT_FOUND = "BUSINESS.CHRONIC_DISEASE.NOT_FOUND";
    }

    public static class Exception
    {
        public const string VALIDATION_ERROR = "EXCEPTION.VALIDATION.ERROR";
        public const string UNAUTHORIZED_ACCESS = "EXCEPTION.UNAUTHORIZED.ACCESS";
        public const string NOT_FOUND = "EXCEPTION.NOT_FOUND";
        public const string OPERATION_NOT_ALLOWED = "EXCEPTION.OPERATION.NOT_ALLOWED";
        public const string INVALID_ARGUMENT = "EXCEPTION.ARGUMENT.INVALID";
        public const string INTERNAL_ERROR = "EXCEPTION.INTERNAL.ERROR";
    }

    public static class Controller
    {
        public const string ID_MISMATCH = "CONTROLLER.ID.MISMATCH";
    }

    public static class Email
    {
        public const string CONFIRMATION_SUBJECT = "EMAIL.CONFIRMATION.SUBJECT";
        public const string CONFIRMATION_GREETING = "EMAIL.CONFIRMATION.GREETING";
        public const string CONFIRMATION_THANK_YOU = "EMAIL.CONFIRMATION.THANK_YOU";
        public const string CONFIRMATION_INSTRUCTION = "EMAIL.CONFIRMATION.INSTRUCTION";
        public const string CONFIRMATION_BUTTON = "EMAIL.CONFIRMATION.BUTTON";
        public const string CONFIRMATION_LINK_INSTRUCTION = "EMAIL.CONFIRMATION.LINK_INSTRUCTION";
        public const string CONFIRMATION_EXPIRY_WARNING = "EMAIL.CONFIRMATION.EXPIRY_WARNING";
        public const string CONFIRMATION_IGNORE_MESSAGE = "EMAIL.CONFIRMATION.IGNORE_MESSAGE";
        public const string EMAIL_FOOTER_COPYRIGHT = "EMAIL.FOOTER.COPYRIGHT";
        public const string EMAIL_FOOTER_AUTOMATED = "EMAIL.FOOTER.AUTOMATED";
    }

    // Essential field validation codes
    public static class Fields
    {
        public const string FULL_NAME_REQUIRED = "FIELD.FULL_NAME.REQUIRED";
        public const string FULL_NAME_MIN_LENGTH = "FIELD.FULL_NAME.MIN_LENGTH";
        public const string FULL_NAME_MAX_LENGTH = "FIELD.FULL_NAME.MAX_LENGTH";
        public const string FULL_NAME_INVALID_CHARACTERS = "FIELD.FULL_NAME.INVALID_CHARACTERS";
        
        public const string EMAIL_REQUIRED = "FIELD.EMAIL.REQUIRED";
        public const string EMAIL_INVALID_FORMAT = "FIELD.EMAIL.INVALID_FORMAT";
        public const string EMAIL_MAX_LENGTH = "FIELD.EMAIL.MAX_LENGTH";
        
        public const string PASSWORD_REQUIRED = "FIELD.PASSWORD.REQUIRED";
        public const string PASSWORD_MIN_LENGTH = "FIELD.PASSWORD.MIN_LENGTH";
        public const string PASSWORD_MAX_LENGTH = "FIELD.PASSWORD.MAX_LENGTH";
        public const string PASSWORD_COMPLEXITY = "FIELD.PASSWORD.COMPLEXITY";
        public const string PASSWORD_NO_SPACES = "FIELD.PASSWORD.NO_SPACES";
        
        public const string CONFIRM_PASSWORD_REQUIRED = "FIELD.CONFIRM_PASSWORD.REQUIRED";
        public const string PASSWORDS_MUST_MATCH = "FIELD.PASSWORDS.MUST_MATCH";
        
        public const string CURRENT_PASSWORD_REQUIRED = "FIELD.CURRENT_PASSWORD.REQUIRED";
        public const string NEW_PASSWORD_REQUIRED = "FIELD.NEW_PASSWORD.REQUIRED";
    }
}