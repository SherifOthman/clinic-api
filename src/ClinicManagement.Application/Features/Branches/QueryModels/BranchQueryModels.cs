namespace ClinicManagement.Application.Features.Branches.QueryModels;

public record BranchRow(
    Guid Id, string Name, string? AddressLine,
    int? StateGeonameId, int? CityGeonameId,
    bool IsMainBranch, bool IsActive,
    List<string> PhoneNumbers
);
