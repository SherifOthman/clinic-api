using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Permissions.Queries;

public record GetRoleDefaultPermissionsQuery : IRequest<Result<Dictionary<string, List<string>>>>;

public class GetRoleDefaultPermissionsHandler
    : IRequestHandler<GetRoleDefaultPermissionsQuery, Result<Dictionary<string, List<string>>>>
{
    private readonly IUnitOfWork _uow;
    public GetRoleDefaultPermissionsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Dictionary<string, List<string>>>> Handle(
        GetRoleDefaultPermissionsQuery request, CancellationToken cancellationToken)
    {
        var defaults = await _uow.Permissions.GetAllRoleDefaultsAsync(cancellationToken);

        var result = defaults.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value.Select(p => p.ToString()).ToList());

        return Result.Success(result);
    }
}
