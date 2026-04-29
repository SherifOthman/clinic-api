using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetDoctorVisitTypesQuery(Guid StaffId, Guid BranchId)
    : IRequest<Result<List<DoctorVisitTypeDto>>>;

public record DoctorVisitTypeDto(
    Guid Id,
    string Name,
    decimal Price,
    bool IsActive
);
