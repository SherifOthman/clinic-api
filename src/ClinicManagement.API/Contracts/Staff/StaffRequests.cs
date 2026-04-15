namespace ClinicManagement.API.Contracts.Staff;

public record InviteStaffRequest(
    string Role,
    string Email,
    Guid? SpecializationId = null);

public record AcceptInvitationRequest(
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string PhoneNumber,
    string Gender);

public record SetOwnerAsDoctorRequest(Guid SpecializationId);

public record SetStaffActiveStatusRequest(bool IsActive);

public record SaveWorkingDaysRequest(Guid BranchId, List<WorkingDayRequest> Days);

public record WorkingDayRequest(
    int Day,
    string StartTime,
    string EndTime,
    bool IsAvailable);

public record UpsertDoctorVisitTypeRequest(
    Guid BranchId,
    Guid? VisitTypeId,
    string NameAr,
    string NameEn,
    decimal Price,
    bool IsActive = true);

public record SetDoctorScheduleLockRequest(bool CanSelfManage);
