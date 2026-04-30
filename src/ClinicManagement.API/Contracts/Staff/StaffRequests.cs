namespace ClinicManagement.API.Contracts.Staff;

public record AcceptInvitationRequest(
    string FullName,
    string UserName,
    string Password,
    string PhoneNumber,
    string Gender);

public record SetOwnerAsDoctorRequest(Guid? SpecializationId);

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
    string Name,
    decimal Price,
    bool IsActive = true);

public record SetDoctorScheduleLockRequest(bool CanSelfManage);
