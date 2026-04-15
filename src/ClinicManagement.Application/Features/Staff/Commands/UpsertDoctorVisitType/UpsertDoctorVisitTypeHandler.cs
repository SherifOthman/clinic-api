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
    private readonly ICurrentUserService _currentUser;

    public UpsertDoctorVisitTypeHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(UpsertDoctorVisitTypeCommand request, CancellationToken ct)
    {
        var doctorId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(request.StaffId, ct);
        if (doctorId == Guid.Empty)
            return Result.Failure<Guid>(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        // Authorization: doctor can only edit own schedule if CanSelfManageSchedule = true
        var isOwner = _currentUser.Roles.Contains(UserRoles.ClinicOwner);
        if (!isOwner)
        {
            var staff = await _uow.Staff.GetByUserIdAsync(_currentUser.GetRequiredUserId(), ct);
            if (staff?.Id != request.StaffId)
                return Result.Failure<Guid>(ErrorCodes.FORBIDDEN, "You can only manage your own visit types");

            var doctor = await _uow.DoctorProfiles.GetByIdAsync(doctorId, ct);
            if (doctor is not null && !doctor.CanSelfManageSchedule)
                return Result.Failure<Guid>(ErrorCodes.FORBIDDEN, "Schedule management is locked by the clinic owner");
        }

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
            DoctorId      = doctorId,
            ClinicBranchId = request.BranchId,
            NameAr        = request.NameAr,
            NameEn        = request.NameEn,
            Price         = request.Price,
            IsActive      = request.IsActive,
        };
        _uow.DoctorVisitTypes.Add(visitType);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(visitType.Id);
    }
}
