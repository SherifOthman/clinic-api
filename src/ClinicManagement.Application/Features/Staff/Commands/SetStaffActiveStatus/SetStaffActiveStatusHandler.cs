using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetStaffActiveStatusHandler : IRequestHandler<SetStaffActiveStatusCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public SetStaffActiveStatusHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(SetStaffActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var staff = await _uow.Staff.GetByIdAsync(request.StaffId, cancellationToken);

        if (staff is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Staff member not found");

        staff.IsActive = request.IsActive;
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
