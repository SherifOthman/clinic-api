namespace ClinicManagement.API.Contracts.Staff;

public record InviteStaffRequest(
    string Role,
    string Email,
    Guid? SpecializationId = null
);

public record AcceptInvitationWithRegistrationRequest(
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string PhoneNumber
);

public record SetOwnerAsDoctorRequest(
    Guid? SpecializationId,
    string? LicenseNumber,
    int? YearsOfExperience
);
