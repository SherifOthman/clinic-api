using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetStaffActiveStatusHandler : IRequestHandler<SetStaffActiveStatusCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ISecurityAuditWriter _auditWriter;

    public SetStaffActiveStatusHandler(IUnitOfWork uow, ICurrentUserService currentUser, ISecurityAuditWriter auditWriter)
    {
        _uow         = uow;
        _currentUser = currentUser;
        _auditWriter = auditWriter;
    }

    public async Task<Result> Handle(SetStaffActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var member = await _uow.Members.GetByIdAsync(request.StaffId, cancellationToken);
        if (member is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Staff member not found");

        var previousStatus = member.IsActive;
        member.IsActive = request.IsActive;
        await _uow.SaveChangesAsync(cancellationToken);

        var action = request.IsActive ? "StaffActivated" : "StaffDeactivated";
        await _auditWriter.WriteAsync(
            _currentUser.UserId, _currentUser.FullName, _currentUser.Username, _currentUser.Email,
            _currentUser.Roles.FirstOrDefault(), member.ClinicId,
            action, $"StaffId: {request.StaffId} | Was: {(previousStatus ? "Active" : "Inactive")}", cancellationToken);

        return Result.Success();
    }
}
