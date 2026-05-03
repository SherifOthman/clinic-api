namespace ClinicManagement.Application.Common.Models.Filters;

/// <summary>Filter + sort parameters for staff list queries.</summary>
public record StaffFilter(
    string? Role          = null,
    bool?   IsActive      = null,
    string? SortBy        = null,
    string? SortDirection = null
);

/// <summary>Filter + sort parameters for invitation list queries.</summary>
public record InvitationFilter(
    Domain.Enums.InvitationStatus? Status       = null,
    string?                        Role         = null,
    string?                        SortBy       = null,
    string?                        SortDirection = null
);
