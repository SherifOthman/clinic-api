namespace ClinicManagement.Application.Features.Branches.QueryModels;

public record BranchRow(
    Guid Id, string Name, string? AddressLine,
    string? CityNameEn, string? CityNameAr,
    string? StateNameEn, string? StateNameAr,
    bool IsMainBranch, bool IsActive,
    List<string> PhoneNumbers
);
