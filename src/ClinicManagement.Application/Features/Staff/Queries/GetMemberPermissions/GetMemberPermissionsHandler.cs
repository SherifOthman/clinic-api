using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetMemberPermissionsHandler : IRequestHandler<GetMemberPermissionsQuery, Result<List<Permission>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetMemberPermissionsHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<List<Permission>>> Handle(GetMemberPermissionsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var member = await _uow.Members.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null || member.ClinicId != clinicId)
            return Result.Failure<List<Permission>>(ErrorCodes.NOT_FOUND, "Staff member not found");

        var permissions = await _uow.Permissions.GetByMemberIdAsync(request.MemberId, cancellationToken);
        return Result.Success(permissions);
    }
}
