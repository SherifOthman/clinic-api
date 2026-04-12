using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<PaginatedResult<AuditLog>> GetProjectedPageAsync(
        string? entityType,
        string? entityId,
        AuditAction? action,
        string? userSearch,
        Guid? clinicId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<Guid?> ResolveClinicIdAsync(string clinicSearch, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetClinicNamesByIdsAsync(List<Guid> clinicIds, CancellationToken ct = default);
}
