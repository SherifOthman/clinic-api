using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetDoctorVisitTypesHandler : IRequestHandler<GetDoctorVisitTypesQuery, Result<List<DoctorVisitTypeDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetDoctorVisitTypesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<DoctorVisitTypeDto>>> Handle(GetDoctorVisitTypesQuery request, CancellationToken ct)
    {
        var doctorId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(request.StaffId, ct);
        if (doctorId == Guid.Empty)
            return Result.Failure<List<DoctorVisitTypeDto>>(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        var items = await _uow.DoctorVisitTypes.GetByDoctorAndBranchAsync(doctorId, request.BranchId, ct);
        var dtos = items.Select(v => new DoctorVisitTypeDto(v.Id, v.NameAr, v.NameEn, v.Price, v.IsActive)).ToList();
        return Result.Success(dtos);
    }
}
