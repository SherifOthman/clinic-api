using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SaveWorkingDaysHandler : IRequestHandler<SaveWorkingDaysCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public SaveWorkingDaysHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(SaveWorkingDaysCommand request, CancellationToken cancellationToken)
    {
        var doctorProfileId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(request.StaffId, cancellationToken);

        if (doctorProfileId == Guid.Empty)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found for this staff member");

        // Replace working days for this specific branch only
        var existing = await _uow.WorkingDays.GetEntitiesByDoctorProfileIdAsync(doctorProfileId, cancellationToken);
        var branchExisting = existing.Where(d => d.ClinicBranchId == request.BranchId).ToList();
        _uow.WorkingDays.RemoveRange(branchExisting);

        foreach (var day in request.Days)
        {
            _uow.WorkingDays.Add(new DoctorWorkingDay
            {
                DoctorId       = doctorProfileId,
                ClinicBranchId = request.BranchId,
                Day            = (DayOfWeek)day.Day,
                StartTime      = TimeOnly.Parse(day.StartTime),
                EndTime        = TimeOnly.Parse(day.EndTime),
                IsAvailable    = day.IsAvailable,
            });
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
