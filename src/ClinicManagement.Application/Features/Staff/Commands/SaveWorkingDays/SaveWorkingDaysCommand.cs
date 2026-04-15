using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record SaveWorkingDaysCommand(
    Guid StaffId,
    Guid BranchId,
    List<WorkingDayInput> Days
) : IRequest<Result>;

public record WorkingDayInput(
    int Day,
    string StartTime,
    string EndTime,
    bool IsAvailable,
    int? MaxAppointmentsPerDay = null
);
