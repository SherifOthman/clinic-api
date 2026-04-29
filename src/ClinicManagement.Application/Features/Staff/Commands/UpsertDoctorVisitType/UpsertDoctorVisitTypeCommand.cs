using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

/// <summary>
/// Creates or updates a visit type for a doctor at a specific branch.
/// Can be called by the clinic owner (any doctor) or the doctor themselves (own profile only,
/// unless CanSelfManageSchedule = false).
/// </summary>
public record UpsertDoctorVisitTypeCommand(
    Guid StaffId,
    Guid BranchId,
    Guid? VisitTypeId,
    string Name,
    decimal Price,
    bool IsActive = true
) : IRequest<Result<Guid>>;
