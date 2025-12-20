namespace ClinicManagement.Application.Common.Constants;

/// <summary>
/// Centralized error messages for API responses
/// </summary>
public static class ErrorMessages
{
    // ============================================================================
    // GENERAL ERROR MESSAGES
    // ============================================================================
    
    public const string ServerError = "An unexpected error occurred. Please try again later.";
    public const string InvalidRequest = "The request format is invalid.";
    public const string NotFound = "The requested resource was not found.";
    public const string Unauthorized = "You are not authorized to access this resource.";
    public const string Forbidden = "Access is forbidden.";
    public const string ValidationFailed = "Validation failed. Please check your input.";

    // ============================================================================
    // AUTHENTICATION ERROR MESSAGES
    // ============================================================================
    
    public const string InvalidCredentials = "Invalid email or password.";
    public const string EmailExists = "An account with this email already exists.";
    public const string UsernameExists = "This username is already taken.";
    public const string InvalidEmail = "Please enter a valid email address.";
    public const string WeakPassword = "Password is too weak. Use at least 8 characters with numbers and letters.";
    public const string PasswordMismatch = "Passwords do not match.";
    public const string TokenExpired = "Your session has expired. Please log in again.";
    public const string InvalidToken = "Invalid authentication token.";
    public const string EmailNotVerified = "Please verify your email address.";
    public const string AccountLocked = "Your account has been locked. Please contact support.";
    public const string AccountDisabled = "Your account has been disabled.";
    public const string InvalidResetToken = "Invalid or expired reset token.";
    public const string UserNotFound = "User not found.";
    public const string CurrentPasswordIncorrect = "Current password is incorrect.";
    public const string InvalidRefreshToken = "Invalid refresh token.";
    public const string ClinicNotFound = "Clinic not found.";

    // ============================================================================
    // FIELD-SPECIFIC ERROR MESSAGES
    // ============================================================================
    
    public const string EmailRequired = "Email is required.";
    public const string EmailInvalid = "Please enter a valid email address.";
    public const string PasswordRequired = "Password is required.";
    public const string PasswordTooShort = "Password must be at least 8 characters long.";
    public const string PasswordTooLong = "Password is too long.";
    public const string FirstNameRequired = "First name is required.";
    public const string LastNameRequired = "Last name is required.";
    public const string NameTooLong = "Name is too long.";
    public const string PhoneInvalid = "Please enter a valid phone number.";
    public const string DateInvalid = "Please enter a valid date.";
    public const string FieldRequired = "This field is required.";
    public const string FieldTooShort = "This field is too short.";
    public const string FieldTooLong = "This field is too long.";
    public const string FieldInvalid = "This field is invalid.";

    // ============================================================================
    // PATIENT ERROR MESSAGES
    // ============================================================================
    
    public const string PatientNotFound = "Patient not found.";
    public const string PatientAlreadyExists = "Patient already exists.";
    public const string PatientHasAppointments = "Cannot delete patient with existing appointments.";

    // ============================================================================
    // APPOINTMENT ERROR MESSAGES
    // ============================================================================
    
    public const string AppointmentNotFound = "Appointment not found.";
    public const string AppointmentSlotUnavailable = "This time slot is not available.";
    public const string AppointmentTimeConflict = "This appointment conflicts with another appointment.";
    public const string AppointmentCannotCancelPast = "Cannot cancel past appointments.";

    // ============================================================================
    // CONTACT ERROR MESSAGES
    // ============================================================================
    
    public const string ContactSubmissionFailed = "Failed to send message. Please try again.";
    public const string ContactMessageRequired = "Message is required.";
    public const string ContactMessageTooLong = "Message is too long.";
}
