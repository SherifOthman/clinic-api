using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Common.Models.Filters;

/// <summary>Filter parameters for audit log queries.</summary>
public record AuditLogFilter(
    string?         EntityType   = null,
    string?         EntityId     = null,
    AuditAction?    Action       = null,
    DateTimeOffset? From         = null,
    DateTimeOffset? To           = null,
    string?         UserSearch   = null,
    string?         ClinicSearch = null
);
