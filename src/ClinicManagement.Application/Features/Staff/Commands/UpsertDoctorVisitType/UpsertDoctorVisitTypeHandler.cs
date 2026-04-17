using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class UpsertDoctorVisitTypeHandler : IRequestHandler<UpsertDoctorVisitTypeCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _permissions;

    public UpsertDoctorVisitTypeHandler(IUnitOfWork uow, IPermissionService permissions)
    {
        _uow         = uow;
        _permissions = permissions;
    }

    public async Task<Result<Guid>> Handle(UpsertDoctorVisitTypeCommand request, CancellationToken ct)
    {
        var doctorId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(request.StaffId, ct);
        if (doctorId == Guid.Empty)
            return Result.Failure<Guid>(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        var permission = await _permissions.CanManageVisitTypesAsync(request.StaffId, ct);
        if (!permission.IsAllowed)
            return Result.Failure<Guid>(ErrorCodes.FORBIDDEN, permission.DeniedReason!);

        // Update existing
        if (request.VisitTypeId.HasValue)
        {
            var existing = await _uow.DoctorVisitTypes.GetByIdAsync(request.VisitTypeId.Value, ct);
            if (existing is null || existing.DoctorId != doctorId)
                return Result.Failure<Guid>(ErrorCodes.NOT_FOUND, "Visit type not found");

            existing.NameAr   = request.NameAr;
            existing.NameEn   = request.NameEn;
            existing.Price    = request.Price;
            existing.IsActive = request.IsActive;
            await _uow.SaveChangesAsync(ct);
            return Result.Success(existing.Id);
        }

        // Create new
        var visitType = new DoctorVisitType
        {
            DoctorId       = doctorId,
            ClinicBranchId = request.BranchId,
            NameAr         = request.NameAr,
            NameEn         = request.NameEn,
            Price          = request.Price,
            IsActive       = request.IsActive,
        };
        _uow.DoctorVisitTypes.Add(visitType);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(visitType.Id);
    }
}
