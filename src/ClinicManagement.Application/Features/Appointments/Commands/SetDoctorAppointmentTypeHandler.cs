using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class SetDoctorAppointmentTypeHandler : IRequestHandler<SetDoctorAppointmentTypeCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SetDoctorAppointmentTypeHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetDoctorAppointmentTypeCommand request, CancellationToken ct)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        // Load the ClinicMember with DoctorInfo — must belong to this clinic
        var member = await _uow.Members.GetByIdWithDoctorInfoAsync(request.MemberId, ct);
        if (member is null || member.ClinicId != clinicId || member.DoctorInfo is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor not found");

        member.DoctorInfo.AppointmentType = request.AppointmentType;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
