using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<PaginatedResult<AuditLog>> GetProjectedPageAsync(
        AuditLogFilter filter,
        Guid? resolvedClinicId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<Guid?> ResolveClinicIdAsync(string clinicSearch, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetClinicNamesByIdsAsync(List<Guid> clinicIds, CancellationToken ct = default);
}
