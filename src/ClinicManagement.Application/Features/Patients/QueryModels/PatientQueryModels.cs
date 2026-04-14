namespace ClinicManagement.Application.Features.Patients.QueryModels;

/// <summary>Flat row returned by the patients list query.</summary>
public record PatientListRow(
    string Id, string PatientCode, string FullName, DateOnly DateOfBirth,
    string Gender, string? BloodType, int ChronicDiseaseCount,
    string? PrimaryPhone, DateTimeOffset CreatedAt, Guid ClinicId, string? ClinicName,
    int? CountryGeonameId, int? StateGeonameId, int? CityGeonameId,
    string? CountryNameEn, string? CountryNameAr,
    string? StateNameEn,   string? StateNameAr,
    string? CityNameEn,    string? CityNameAr
);

/// <summary>Flat row returned by the recent patients dashboard query.</summary>
public record RecentPatientRow(
    string Id, string PatientCode, string FullName,
    DateOnly DateOfBirth, string Gender, DateTimeOffset CreatedAt
);

/// <summary>Full patient detail including phones, diseases, and audit user names.</summary>
public record PatientDetailData(
    Guid Id, string PatientCode, string FullName, DateOnly DateOfBirth,
    string Gender, string? BloodType,
    int? CountryGeonameId, int? StateGeonameId, int? CityGeonameId,
    string? CountryNameEn, string? CountryNameAr,
    string? StateNameEn,   string? StateNameAr,
    string? CityNameEn,    string? CityNameAr,
    Guid ClinicId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    Guid? CreatedBy, Guid? UpdatedBy,
    List<string> Phones, List<PatientDiseaseRow> Diseases,
    Dictionary<Guid, string> AuditUserNames, string? ClinicName
);

public record PatientDiseaseRow(string Id, string NameEn, string NameAr);
