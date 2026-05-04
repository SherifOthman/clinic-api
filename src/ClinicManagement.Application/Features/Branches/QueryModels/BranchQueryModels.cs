namespace ClinicManagement.Application.Features.Branches.QueryModels;

public record BranchPhoneRow(string PhoneNumber, string? Label);

public record BranchRow(
    Guid Id, string Name, string? AddressLine,
    int? StateGeonameId, int? CityGeonameId,
    bool IsMainBranch, bool IsActive,
    List<BranchPhoneRow> PhoneNumbers
);
