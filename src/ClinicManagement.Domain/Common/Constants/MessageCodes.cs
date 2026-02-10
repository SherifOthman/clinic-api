namespace ClinicManagement.Domain.Common.Constants;

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
        public const string USER_CREATION_FAILED = "AUTH.USER.CREATION_FAILED";
        public const string ROLE_ASSIGNMENT_FAILED = "AUTH.ROLE.ASSIGNMENT_FAILED";
        public const string REGISTRATION_FAILED = "AUTH.REGISTRATION.FAILED";
        public const string USER_NO_CLINIC = "AUTH.USER.NO_CLINIC";
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
        public const string PROFILE_IMAGE_REQUIRED = "VALIDATION.PROFILE_IMAGE.REQUIRED";
        public const string PROFILE_IMAGE_SIZE_EXCEEDED = "VALIDATION.PROFILE_IMAGE.SIZE_EXCEEDED";
        public const string PROFILE_IMAGE_INVALID_TYPE = "VALIDATION.PROFILE_IMAGE.INVALID_TYPE";
    }

    public static class Business
    {
        public const string ENTITY_NOT_FOUND = "BUSINESS.ENTITY.NOT_FOUND";
        public const string ENTITY_ALREADY_EXISTS = "BUSINESS.ENTITY.ALREADY_EXISTS";
        public const string OPERATION_NOT_ALLOWED = "BUSINESS.OPERATION.NOT_ALLOWED";
        public const string INVALID_OPERATION = "BUSINESS.OPERATION.INVALID";
        public const string CHRONIC_DISEASE_NOT_FOUND = "BUSINESS.CHRONIC_DISEASE.NOT_FOUND";
        public const string CHRONIC_DISEASE_ALREADY_EXISTS = "BUSINESS.CHRONIC_DISEASE.ALREADY_EXISTS";
    }

    public static class Invitation
    {
        public const string INVALID_TOKEN = "INVITATION.INVALID_TOKEN";
        public const string ALREADY_ACCEPTED = "INVITATION.ALREADY_ACCEPTED";
        public const string EXPIRED = "INVITATION.EXPIRED";
        public const string ALREADY_PENDING = "INVITATION.ALREADY_PENDING";
    }

    // Pharmacy and Billing specific codes
    public static class Medicine
    {
        public const string NOT_FOUND = "MEDICINE.NOT_FOUND";
        public const string ALREADY_EXISTS = "MEDICINE.ALREADY_EXISTS";
        public const string INSUFFICIENT_STOCK = "MEDICINE.INSUFFICIENT_STOCK";
        public const string LOW_STOCK_WARNING = "MEDICINE.LOW_STOCK_WARNING";
        public const string INVALID_STOCK_QUANTITY = "MEDICINE.INVALID_STOCK_QUANTITY";
        public const string INVALID_PRICE = "MEDICINE.INVALID_PRICE";
        public const string INVALID_STRIPS_PER_BOX = "MEDICINE.INVALID_STRIPS_PER_BOX";
        public const string NAME_REQUIRED = "MEDICINE.NAME.REQUIRED";
        public const string NAME_TOO_LONG = "MEDICINE.NAME.TOO_LONG";
        public const string PRICE_MUST_BE_POSITIVE = "MEDICINE.PRICE.MUST_BE_POSITIVE";
        public const string STOCK_CANNOT_BE_NEGATIVE = "MEDICINE.STOCK.CANNOT_BE_NEGATIVE";
        public const string STRIPS_PER_BOX_MUST_BE_POSITIVE = "MEDICINE.STRIPS_PER_BOX.MUST_BE_POSITIVE";
        public const string MINIMUM_STOCK_CANNOT_BE_NEGATIVE = "MEDICINE.MINIMUM_STOCK.CANNOT_BE_NEGATIVE";
    }

    public static class MedicalSupply
    {
        public const string NOT_FOUND = "MEDICAL_SUPPLY.NOT_FOUND";
        public const string ALREADY_EXISTS = "MEDICAL_SUPPLY.ALREADY_EXISTS";
        public const string INSUFFICIENT_STOCK = "MEDICAL_SUPPLY.INSUFFICIENT_STOCK";
        public const string LOW_STOCK_WARNING = "MEDICAL_SUPPLY.LOW_STOCK_WARNING";
        public const string INVALID_STOCK_QUANTITY = "MEDICAL_SUPPLY.INVALID_STOCK_QUANTITY";
        public const string INVALID_PRICE = "MEDICAL_SUPPLY.INVALID_PRICE";
        public const string NAME_REQUIRED = "MEDICAL_SUPPLY.NAME.REQUIRED";
        public const string NAME_TOO_LONG = "MEDICAL_SUPPLY.NAME.TOO_LONG";
        public const string PRICE_MUST_BE_POSITIVE = "MEDICAL_SUPPLY.PRICE.MUST_BE_POSITIVE";
        public const string STOCK_CANNOT_BE_NEGATIVE = "MEDICAL_SUPPLY.STOCK.CANNOT_BE_NEGATIVE";
        public const string MINIMUM_STOCK_CANNOT_BE_NEGATIVE = "MEDICAL_SUPPLY.MINIMUM_STOCK.CANNOT_BE_NEGATIVE";
    }

    public static class MedicalService
    {
        public const string NOT_FOUND = "MEDICAL_SERVICE.NOT_FOUND";
        public const string ALREADY_EXISTS = "MEDICAL_SERVICE.ALREADY_EXISTS";
        public const string NAME_REQUIRED = "MEDICAL_SERVICE.NAME.REQUIRED";
        public const string NAME_TOO_LONG = "MEDICAL_SERVICE.NAME.TOO_LONG";
        public const string PRICE_MUST_BE_POSITIVE = "MEDICAL_SERVICE.PRICE.MUST_BE_POSITIVE";
    }

    public static class Invoice
    {
        public const string NOT_FOUND = "INVOICE.NOT_FOUND";
        public const string ALREADY_PAID = "INVOICE.ALREADY_PAID";
        public const string INVALID_STATUS = "INVOICE.INVALID_STATUS";
        public const string INVALID_AMOUNT = "INVOICE.INVALID_AMOUNT";
        public const string INVALID_DISCOUNT = "INVOICE.INVALID_DISCOUNT";
        public const string EMPTY_ITEMS = "INVOICE.EMPTY_ITEMS";
        public const string INVALID_ITEM_QUANTITY = "INVOICE.INVALID_ITEM_QUANTITY";
        public const string INVALID_ITEM_PRICE = "INVOICE.INVALID_ITEM_PRICE";
        public const string PATIENT_REQUIRED = "INVOICE.PATIENT.REQUIRED";
        public const string CLINIC_REQUIRED = "INVOICE.CLINIC.REQUIRED";
        public const string DISCOUNT_CANNOT_BE_NEGATIVE = "INVOICE.DISCOUNT.CANNOT_BE_NEGATIVE";
        public const string DISCOUNT_EXCEEDS_TOTAL = "INVOICE.DISCOUNT.EXCEEDS_TOTAL";
    }

    public static class Payment
    {
        public const string NOT_FOUND = "PAYMENT.NOT_FOUND";
        public const string INVALID_AMOUNT = "PAYMENT.INVALID_AMOUNT";
        public const string AMOUNT_EXCEEDS_REMAINING = "PAYMENT.AMOUNT_EXCEEDS_REMAINING";
        public const string INVOICE_REQUIRED = "PAYMENT.INVOICE.REQUIRED";
        public const string AMOUNT_MUST_BE_POSITIVE = "PAYMENT.AMOUNT.MUST_BE_POSITIVE";
        public const string INVALID_PAYMENT_METHOD = "PAYMENT.INVALID_PAYMENT_METHOD";
        public const string REFERENCE_NUMBER_TOO_LONG = "PAYMENT.REFERENCE_NUMBER.TOO_LONG";
        public const string NOTE_TOO_LONG = "PAYMENT.NOTE.TOO_LONG";
    }

    public static class Measurement
    {
        public const string ATTRIBUTE_NOT_FOUND = "MEASUREMENT.ATTRIBUTE.NOT_FOUND";
        public const string ATTRIBUTE_ALREADY_EXISTS = "MEASUREMENT.ATTRIBUTE.ALREADY_EXISTS";
        public const string INVALID_DATA_TYPE = "MEASUREMENT.INVALID_DATA_TYPE";
        public const string INVALID_VALUE_FOR_TYPE = "MEASUREMENT.INVALID_VALUE_FOR_TYPE";
        public const string NAME_REQUIRED = "MEASUREMENT.NAME.REQUIRED";
        public const string NAME_TOO_LONG = "MEASUREMENT.NAME.TOO_LONG";
        public const string DESCRIPTION_TOO_LONG = "MEASUREMENT.DESCRIPTION.TOO_LONG";
        public const string VALUE_REQUIRED = "MEASUREMENT.VALUE.REQUIRED";
    }

    public static class Appointment
    {
        public const string NOT_FOUND = "APPOINTMENT.NOT_FOUND";
        public const string PATIENT_REQUIRED = "APPOINTMENT.PATIENT.REQUIRED";
        public const string DOCTOR_REQUIRED = "APPOINTMENT.DOCTOR.REQUIRED";
        public const string DATE_REQUIRED = "APPOINTMENT.DATE.REQUIRED";
        public const string DATE_IN_PAST = "APPOINTMENT.DATE.IN_PAST";
        public const string TYPE_REQUIRED = "APPOINTMENT.TYPE.REQUIRED";
        public const string PRICE_CANNOT_BE_NEGATIVE = "APPOINTMENT.PRICE.CANNOT_BE_NEGATIVE";
        public const string DISCOUNT_CANNOT_BE_NEGATIVE = "APPOINTMENT.DISCOUNT.CANNOT_BE_NEGATIVE";
        public const string PAID_AMOUNT_CANNOT_BE_NEGATIVE = "APPOINTMENT.PAID_AMOUNT.CANNOT_BE_NEGATIVE";
        public const string QUEUE_NUMBER_CONFLICT = "APPOINTMENT.QUEUE_NUMBER.CONFLICT";
    }

    public static class Patient
    {
        public const string NOT_FOUND = "PATIENT.NOT_FOUND";
        public const string ALREADY_EXISTS = "PATIENT.ALREADY_EXISTS";
        public const string ID_REQUIRED = "PATIENT.ID.REQUIRED";
    }

    public static class ChronicDisease
    {
        public const string NOT_FOUND = "CHRONIC_DISEASE.NOT_FOUND";
        public const string ALREADY_EXISTS = "CHRONIC_DISEASE.ALREADY_EXISTS";
        public const string STATUS_TOO_LONG = "CHRONIC_DISEASE.STATUS.TOO_LONG";
        public const string NOTES_TOO_LONG = "CHRONIC_DISEASE.NOTES.TOO_LONG";
        public const string DIAGNOSED_DATE_IN_FUTURE = "CHRONIC_DISEASE.DIAGNOSED_DATE.IN_FUTURE";
    }

    public static class Common
    {
        public const string CLINIC_BRANCH_REQUIRED = "COMMON.CLINIC_BRANCH.REQUIRED";
        public const string CLINIC_BRANCH_NOT_FOUND = "COMMON.CLINIC_BRANCH.NOT_FOUND";
        public const string INVALID_GUID = "COMMON.INVALID_GUID";
        public const string PAGINATION_INVALID_PAGE_NUMBER = "COMMON.PAGINATION.INVALID_PAGE_NUMBER";
        public const string PAGINATION_INVALID_PAGE_SIZE = "COMMON.PAGINATION.INVALID_PAGE_SIZE";
        public const string SEARCH_TERM_TOO_LONG = "COMMON.SEARCH_TERM.TOO_LONG";
    }

    public static class Exception
    {
        public const string VALIDATION_ERROR = "EXCEPTION.VALIDATION.ERROR";
        public const string UNAUTHORIZED_ACCESS = "EXCEPTION.UNAUTHORIZED.ACCESS";
        public const string NOT_FOUND = "EXCEPTION.NOT_FOUND";
        public const string OPERATION_NOT_ALLOWED = "EXCEPTION.OPERATION.NOT_ALLOWED";
        public const string INVALID_ARGUMENT = "EXCEPTION.ARGUMENT.INVALID";
        public const string INTERNAL_ERROR = "EXCEPTION.INTERNAL.ERROR";
        public const string INTERNAL_SERVER_ERROR = "EXCEPTION.INTERNAL.SERVER_ERROR";
    }

    public static class Domain
    {
        public const string INVALID_BUSINESS_OPERATION = "DOMAIN.BUSINESS.INVALID_OPERATION";
        public const string INSUFFICIENT_STOCK = "DOMAIN.STOCK.INSUFFICIENT";
        public const string INVALID_DISCOUNT = "DOMAIN.DISCOUNT.INVALID";
        public const string EXPIRED_MEDICINE = "DOMAIN.MEDICINE.EXPIRED";
        public const string DISCONTINUED_MEDICINE = "DOMAIN.MEDICINE.DISCONTINUED";
        public const string INVALID_INVOICE_STATE = "DOMAIN.INVOICE.INVALID_STATE";
        public const string INVALID_EXPIRY_DATE = "DOMAIN.MEDICINE.INVALID_EXPIRY_DATE";
        public const string EMPTY_INVOICE_ITEMS = "DOMAIN.INVOICE.EMPTY_ITEMS";
        public const string INVOICE_ALREADY_PAID = "DOMAIN.INVOICE.ALREADY_PAID";
        public const string INVOICE_CANCELLED = "DOMAIN.INVOICE.CANCELLED";
        public const string MEDICINE_VALIDATION_FAILED = "DOMAIN.MEDICINE.VALIDATION_FAILED";
        public const string INVOICE_VALIDATION_FAILED = "DOMAIN.INVOICE.VALIDATION_FAILED";
        
        // Appointment domain codes
        public const string INVALID_APPOINTMENT_STATE = "DOMAIN.APPOINTMENT.INVALID_STATE";
        public const string INVALID_APPOINTMENT_DATE = "DOMAIN.APPOINTMENT.INVALID_DATE";
        public const string APPOINTMENT_IN_PAST = "DOMAIN.APPOINTMENT.IN_PAST";
        public const string APPOINTMENT_ALREADY_COMPLETED = "DOMAIN.APPOINTMENT.ALREADY_COMPLETED";
        public const string APPOINTMENT_CANCELLED = "DOMAIN.APPOINTMENT.CANCELLED";
        public const string APPOINTMENT_VALIDATION_FAILED = "DOMAIN.APPOINTMENT.VALIDATION_FAILED";
        public const string PAYMENT_EXCEEDS_REMAINING = "DOMAIN.APPOINTMENT.PAYMENT_EXCEEDS_REMAINING";
        
        // Patient domain codes
        public const string INVALID_DATE_OF_BIRTH = "DOMAIN.PATIENT.INVALID_DATE_OF_BIRTH";
        public const string DUPLICATE_PHONE_NUMBER = "DOMAIN.PATIENT.DUPLICATE_PHONE_NUMBER";
        public const string PHONE_NUMBER_NOT_FOUND = "DOMAIN.PATIENT.PHONE_NUMBER_NOT_FOUND";
        public const string DUPLICATE_CHRONIC_DISEASE = "DOMAIN.PATIENT.DUPLICATE_CHRONIC_DISEASE";
        public const string CHRONIC_DISEASE_NOT_FOUND = "DOMAIN.PATIENT.CHRONIC_DISEASE_NOT_FOUND";
        public const string INVALID_DIAGNOSED_DATE = "DOMAIN.PATIENT.INVALID_DIAGNOSED_DATE";
        public const string PATIENT_VALIDATION_FAILED = "DOMAIN.PATIENT.VALIDATION_FAILED";
        
        // Payment domain codes
        public const string INVALID_PAYMENT_STATE = "DOMAIN.PAYMENT.INVALID_STATE";
        public const string INVALID_PAYMENT_DATE = "DOMAIN.PAYMENT.INVALID_DATE";
        public const string INVALID_PAYMENT_AMOUNT = "DOMAIN.PAYMENT.INVALID_AMOUNT";
        public const string REFERENCE_NUMBER_REQUIRED = "DOMAIN.PAYMENT.REFERENCE_NUMBER_REQUIRED";
        public const string PAYMENT_VALIDATION_FAILED = "DOMAIN.PAYMENT.VALIDATION_FAILED";
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
        public const string PASSWORD_DIFFERENT_REQUIRED = "FIELD.PASSWORD.DIFFERENT_REQUIRED";
        
        public const string PHONE_NUMBER_REQUIRED = "FIELD.PHONE_NUMBER.REQUIRED";
        public const string PHONE_NUMBER_MAX_LENGTH = "FIELD.PHONE_NUMBER.MAX_LENGTH";
        public const string PHONE_NUMBER_INVALID = "FIELD.PHONE_NUMBER.INVALID";
        
        public const string PROFILE_IMAGE_URL_REQUIRED = "FIELD.PROFILE_IMAGE_URL.REQUIRED";
        public const string PROFILE_IMAGE_URL_MAX_LENGTH = "FIELD.PROFILE_IMAGE_URL.MAX_LENGTH";
    }
}
