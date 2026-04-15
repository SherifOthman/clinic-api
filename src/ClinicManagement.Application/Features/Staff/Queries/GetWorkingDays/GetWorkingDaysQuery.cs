using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetWorkingDaysQuery(Guid StaffId, Guid? BranchId = null) : IRequest<Result<List<WorkingDayDto>>>;

public record WorkingDayDto(
    int Day,
    string StartTime,
    string EndTime,
    bool IsAvailable,
    Guid BranchId
);
