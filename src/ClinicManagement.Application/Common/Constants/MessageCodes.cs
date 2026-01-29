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
    }

    public static class Authorization
    {
        public const string INSUFFICIENT_PERMISSIONS = "AUTHZ.PERMISSIONS.INSUFFICIENT";
        public const string ACCESS_DENIED = "AUTHZ.ACCESS.DENIED";
        public const string USER_NO_CLINIC_ACCESS = "AUTHZ.CLINIC.NO_ACCESS";
        public const string USER_CLINIC_NOT_FOUND = "AUTHZ.CLINIC.NOT_FOUND";
    }

    public static class Validation
    {
        public const string REQUIRED_FIELD = "VALIDATION.FIELD.REQUIRED";
        public const string INVALID_FORMAT = "VALIDATION.FORMAT.INVALID";
        public const string INVALID_LENGTH = "VALIDATION.LENGTH.INVALID";
        public const string INVALID_EMAIL = "VALIDATION.EMAIL.INVALID";
        public const string INVALID_PHONE = "VALIDATION.PHONE.INVALID";
        public const string EMAIL_ALREADY_REGISTERED = "VALIDATION.EMAIL.ALREADY_REGISTERED";
        public const string USERNAME_ALREADY_TAKEN = "VALIDATION.USERNAME.ALREADY_TAKEN";
        public const string FIELD_REQUIRED = "VALIDATION.FIELD.REQUIRED";
        public const string FIELD_INVALID_FORMAT = "VALIDATION.FIELD.INVALID_FORMAT";
        public const string FIELD_INVALID_LENGTH = "VALIDATION.FIELD.INVALID_LENGTH";
        public const string FIELD_MIN_LENGTH = "VALIDATION.FIELD.MIN_LENGTH";
        public const string FIELD_MAX_LENGTH = "VALIDATION.FIELD.MAX_LENGTH";
        public const string FIELD_INVALID_CHARACTERS = "VALIDATION.FIELD.INVALID_CHARACTERS";
        public const string FIELD_CANNOT_START_END_UNDERSCORE = "VALIDATION.FIELD.UNDERSCORE_POSITION";
        public const string PASSWORD_COMPLEXITY = "VALIDATION.PASSWORD.COMPLEXITY";
        public const string PASSWORD_NO_SPACES = "VALIDATION.PASSWORD.NO_SPACES";
        public const string PASSWORD_DIFFERENT_REQUIRED = "VALIDATION.PASSWORD.DIFFERENT_REQUIRED";
        public const string PASSWORDS_MUST_MATCH = "VALIDATION.PASSWORD.MUST_MATCH";
        public const string DATE_MUST_BE_PAST = "VALIDATION.DATE.MUST_BE_PAST";
        public const string DATE_TOO_OLD = "VALIDATION.DATE.TOO_OLD";
        public const string ENUM_INVALID_VALUE = "VALIDATION.ENUM.INVALID_VALUE";
        public const string POSITIVE_NUMBER_REQUIRED = "VALIDATION.NUMBER.POSITIVE_REQUIRED";
        public const string GENERAL_VALIDATION_ERROR = "VALIDATION.GENERAL.ERROR";
    }

    public static class Business
    {
        public const string ENTITY_NOT_FOUND = "BUSINESS.ENTITY.NOT_FOUND";
        public const string ENTITY_ALREADY_EXISTS = "BUSINESS.ENTITY.ALREADY_EXISTS";
        public const string OPERATION_NOT_ALLOWED = "BUSINESS.OPERATION.NOT_ALLOWED";
        public const string INVALID_OPERATION = "BUSINESS.OPERATION.INVALID";
        public const string PATIENT_NOT_FOUND = "BUSINESS.PATIENT.NOT_FOUND";
        public const string SUBSCRIPTION_PLAN_NOT_FOUND = "BUSINESS.SUBSCRIPTION_PLAN.NOT_FOUND";
        public const string CHRONIC_DISEASE_NOT_FOUND = "BUSINESS.CHRONIC_DISEASE.NOT_FOUND";
    }

    public static class File
    {
        public const string FILE_NOT_FOUND = "FILE.NOT_FOUND";
        public const string FILE_TOO_LARGE = "FILE.TOO_LARGE";
        public const string INVALID_FILE_TYPE = "FILE.INVALID_TYPE";
        public const string FILE_UPLOAD_FAILED = "FILE.UPLOAD_FAILED";
        public const string FILE_DELETE_FAILED = "FILE.DELETE_FAILED";
    }

    public static class Onboarding
    {
        public const string USER_ALREADY_HAS_CLINIC = "ONBOARDING.USER.ALREADY_HAS_CLINIC";
        public const string INVALID_SUBSCRIPTION_PLAN = "ONBOARDING.SUBSCRIPTION_PLAN.INVALID";
        public const string ONBOARDING_INCOMPLETE = "ONBOARDING.INCOMPLETE";
    }

    public static class Appointment
    {
        public const string APPOINTMENT_DATE_IN_PAST = "APPOINTMENT.DATE.IN_PAST";
        public const string APPOINTMENT_NOT_FOUND = "APPOINTMENT.NOT_FOUND";
        public const string APPOINTMENT_ALREADY_CONFIRMED = "APPOINTMENT.ALREADY_CONFIRMED";
        public const string APPOINTMENT_CANNOT_BE_CANCELLED = "APPOINTMENT.CANNOT_BE_CANCELLED";
        public const string INVALID_APPOINTMENT_TIME = "APPOINTMENT.TIME.INVALID";
        public const string DOCTOR_NOT_AVAILABLE = "APPOINTMENT.DOCTOR.NOT_AVAILABLE";
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

    public static class Location
    {
        public const string NOT_FOUND = "LOCATION.NOT_FOUND";
        public const string HIERARCHY_FAILED = "LOCATION.HIERARCHY.FAILED";
        public const string GEONAMES_HEALTH_FAILED = "LOCATION.GEONAMES.HEALTH_FAILED";
        public const string COUNTRIES_API_FAILED = "LOCATION.COUNTRIES_API.FAILED";
        public const string COUNTRIES_API_INVALID_RESPONSE = "LOCATION.COUNTRIES_API.INVALID_RESPONSE";
    }

    public static class Admin
    {
        public const string USERS_RETRIEVAL_FAILED = "ADMIN.USERS.RETRIEVAL_FAILED";
        public const string CLINICS_RETRIEVAL_FAILED = "ADMIN.CLINICS.RETRIEVAL_FAILED";
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
        public const string WELCOME_SUBJECT = "EMAIL.WELCOME.SUBJECT";
        public const string WELCOME_GREETING = "EMAIL.WELCOME.GREETING";
        public const string WELCOME_MESSAGE = "EMAIL.WELCOME.MESSAGE";
        public const string USAGE_WARNING_SUBJECT = "EMAIL.USAGE_WARNING.SUBJECT";
        public const string USAGE_WARNING_MESSAGE = "EMAIL.USAGE_WARNING.MESSAGE";
        public const string SUBSCRIPTION_EXPIRING_SUBJECT = "EMAIL.SUBSCRIPTION_EXPIRING.SUBJECT";
        public const string SUBSCRIPTION_EXPIRING_MESSAGE = "EMAIL.SUBSCRIPTION_EXPIRING.MESSAGE";
        public const string EMAIL_FOOTER_COPYRIGHT = "EMAIL.FOOTER.COPYRIGHT";
        public const string EMAIL_FOOTER_AUTOMATED = "EMAIL.FOOTER.AUTOMATED";
    }

    // Specific field validation codes
    public static class Fields
    {
        public const string FIRST_NAME_REQUIRED = "FIELD.FIRST_NAME.REQUIRED";
        public const string FIRST_NAME_MIN_LENGTH = "FIELD.FIRST_NAME.MIN_LENGTH";
        public const string FIRST_NAME_MAX_LENGTH = "FIELD.FIRST_NAME.MAX_LENGTH";
        public const string FIRST_NAME_INVALID_CHARACTERS = "FIELD.FIRST_NAME.INVALID_CHARACTERS";
        
        public const string LAST_NAME_REQUIRED = "FIELD.LAST_NAME.REQUIRED";
        public const string LAST_NAME_MIN_LENGTH = "FIELD.LAST_NAME.MIN_LENGTH";
        public const string LAST_NAME_MAX_LENGTH = "FIELD.LAST_NAME.MAX_LENGTH";
        public const string LAST_NAME_INVALID_CHARACTERS = "FIELD.LAST_NAME.INVALID_CHARACTERS";
        
        public const string USERNAME_REQUIRED = "FIELD.USERNAME.REQUIRED";
        public const string USERNAME_MIN_LENGTH = "FIELD.USERNAME.MIN_LENGTH";
        public const string USERNAME_MAX_LENGTH = "FIELD.USERNAME.MAX_LENGTH";
        public const string USERNAME_INVALID_CHARACTERS = "FIELD.USERNAME.INVALID_CHARACTERS";
        public const string USERNAME_UNDERSCORE_POSITION = "FIELD.USERNAME.UNDERSCORE_POSITION";
        
        public const string EMAIL_REQUIRED = "FIELD.EMAIL.REQUIRED";
        public const string EMAIL_INVALID_FORMAT = "FIELD.EMAIL.INVALID_FORMAT";
        public const string EMAIL_MAX_LENGTH = "FIELD.EMAIL.MAX_LENGTH";
        
        public const string PASSWORD_REQUIRED = "FIELD.PASSWORD.REQUIRED";
        public const string PASSWORD_MIN_LENGTH = "FIELD.PASSWORD.MIN_LENGTH";
        public const string PASSWORD_MAX_LENGTH = "FIELD.PASSWORD.MAX_LENGTH";
        public const string PASSWORD_COMPLEXITY = "FIELD.PASSWORD.COMPLEXITY";
        public const string PASSWORD_NO_SPACES = "FIELD.PASSWORD.NO_SPACES";
        public const string PASSWORD_DIFFERENT_REQUIRED = "FIELD.PASSWORD.DIFFERENT_REQUIRED";
        
        public const string CONFIRM_PASSWORD_REQUIRED = "FIELD.CONFIRM_PASSWORD.REQUIRED";
        public const string PASSWORDS_MUST_MATCH = "FIELD.PASSWORDS.MUST_MATCH";
        
        public const string PHONE_NUMBER_REQUIRED = "FIELD.PHONE_NUMBER.REQUIRED";
        public const string PHONE_NUMBER_INVALID = "FIELD.PHONE_NUMBER.INVALID";
        
        public const string CURRENT_PASSWORD_REQUIRED = "FIELD.CURRENT_PASSWORD.REQUIRED";
        public const string NEW_PASSWORD_REQUIRED = "FIELD.NEW_PASSWORD.REQUIRED";
        
        public const string CLINIC_NAME_REQUIRED = "FIELD.CLINIC_NAME.REQUIRED";
        public const string CLINIC_NAME_MAX_LENGTH = "FIELD.CLINIC_NAME.MAX_LENGTH";
        public const string CLINIC_NAME_INVALID_CHARACTERS = "FIELD.CLINIC_NAME.INVALID_CHARACTERS";
        
        public const string SUBSCRIPTION_PLAN_REQUIRED = "FIELD.SUBSCRIPTION_PLAN.REQUIRED";
        
        public const string BRANCH_NAME_REQUIRED = "FIELD.BRANCH_NAME.REQUIRED";
        public const string BRANCH_NAME_MAX_LENGTH = "FIELD.BRANCH_NAME.MAX_LENGTH";
        public const string BRANCH_NAME_INVALID_CHARACTERS = "FIELD.BRANCH_NAME.INVALID_CHARACTERS";
        
        public const string BRANCH_ADDRESS_REQUIRED = "FIELD.BRANCH_ADDRESS.REQUIRED";
        public const string BRANCH_ADDRESS_MAX_LENGTH = "FIELD.BRANCH_ADDRESS.MAX_LENGTH";
        
        public const string COUNTRY_REQUIRED = "FIELD.COUNTRY.REQUIRED";
        public const string CITY_REQUIRED = "FIELD.CITY.REQUIRED";
        
        public const string BRANCH_PHONE_NUMBERS_REQUIRED = "FIELD.BRANCH_PHONE_NUMBERS.REQUIRED";
        
        public const string FULL_NAME_REQUIRED = "FIELD.FULL_NAME.REQUIRED";
        public const string FULL_NAME_MIN_LENGTH = "FIELD.FULL_NAME.MIN_LENGTH";
        public const string FULL_NAME_MAX_LENGTH = "FIELD.FULL_NAME.MAX_LENGTH";
        public const string FULL_NAME_INVALID_CHARACTERS = "FIELD.FULL_NAME.INVALID_CHARACTERS";
        
        public const string DATE_OF_BIRTH_PAST = "FIELD.DATE_OF_BIRTH.PAST";
        public const string DATE_OF_BIRTH_TOO_OLD = "FIELD.DATE_OF_BIRTH.TOO_OLD";
        
        public const string GENDER_INVALID = "FIELD.GENDER.INVALID";
        
        public const string PHONE_NUMBERS_REQUIRED = "FIELD.PHONE_NUMBERS.REQUIRED";
        
        public const string CHRONIC_DISEASE_IDS_POSITIVE = "FIELD.CHRONIC_DISEASE_IDS.POSITIVE";
    }
}