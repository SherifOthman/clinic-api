using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetWorkingDaysQuery(Guid StaffId, Guid? BranchId = null) : IRequest<Result<List<WorkingDayDto>>>;

public record WorkingDayDto(
    int Day,           // 0=Sunday … 6=Saturday (DayOfWeek)
    string StartTime,  // "HH:mm"
    string EndTime,    // "HH:mm"
    bool IsAvailable,
    Guid BranchId
);
