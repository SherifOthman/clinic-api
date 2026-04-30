using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicSettings.Commands;

public class UpdateClinicSettingsHandler : IRequestHandler<UpdateClinicSettingsCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateClinicSettingsHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow          = uow;
        _currentUser  = currentUser;
    }

    public async Task<Result> Handle(UpdateClinicSettingsCommand request, CancellationToken ct)
    {
        if (request.WeekStartDay < 0 || request.WeekStartDay > 6)
            return Result.Failure(ErrorCodes.VALIDATION_ERROR, "WeekStartDay must be between 0 (Sunday) and 6 (Saturday).");

        var userId = _currentUser.GetRequiredUserId();
        var clinic = await _uow.Clinics.GetByOwnerIdAsync(userId, ct);
        if (clinic is null)
            return Result.Failure(ErrorCodes.CLINIC_NOT_FOUND, "Clinic not found.");

        clinic.WeekStartDay = request.WeekStartDay;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
