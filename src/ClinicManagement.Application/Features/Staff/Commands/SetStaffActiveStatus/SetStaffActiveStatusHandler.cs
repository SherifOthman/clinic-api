using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetStaffActiveStatusHandler : IRequestHandler<SetStaffActiveStatusCommand, Result>
{
    private readonly IClinicMemberRepository _members;
    private readonly IUnitOfWork             _uow;

    public SetStaffActiveStatusHandler(IClinicMemberRepository members, IUnitOfWork uow)
    {
        _members = members;
        _uow     = uow;
    }

    public async Task<Result> Handle(SetStaffActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var member = await _members.GetByIdAsync(request.StaffId, cancellationToken);
        if (member is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Staff member not found");

        member.IsActive = request.IsActive;
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
