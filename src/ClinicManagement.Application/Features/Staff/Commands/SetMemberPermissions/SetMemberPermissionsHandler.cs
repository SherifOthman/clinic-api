using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetMemberPermissionsHandler : IRequestHandler<SetMemberPermissionsCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SetMemberPermissionsHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetMemberPermissionsCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var member = await _uow.Members.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null || member.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Staff member not found");

        // Owner permissions cannot be modified
        if (member.IsOwner)
            return Result.Failure(ErrorCodes.FORBIDDEN, "Owner permissions cannot be modified");

        await _uow.Permissions.SetPermissionsAsync(request.MemberId, request.Permissions, cancellationToken);
        return Result.Success();
    }
}
