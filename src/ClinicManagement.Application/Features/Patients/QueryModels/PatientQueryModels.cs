namespace ClinicManagement.Application.Features.Patients.QueryModels;

/// <summary>Flat row returned by the patients list query.</summary>
public record PatientListRow(
    string Id, string PatientCode, string FullName, DateOnly? DateOfBirth,
    string Gender, string? BloodType, int ChronicDiseaseCount,
    string? PrimaryPhone, DateTimeOffset CreatedAt, Guid ClinicId, string? ClinicName,
    int? CountryGeonameId, int? StateGeonameId, int? CityGeonameId,
    string? CityNameEn, string? CityNameAr, bool IsDeleted = false
);

/// <summary>Flat row returned by the recent patients dashboard query.</summary>
public record RecentPatientRow(
    string Id, string PatientCode, string FullName,
    DateOnly? DateOfBirth, string Gender, DateTimeOffset CreatedAt
);

/// <summary>Full patient detail — phones, diseases, location names, and audit trail.</summary>
public record PatientDetailData(
    Guid Id, string PatientCode,
    string FullName,
    DateOnly? DateOfBirth,
    string Gender, string? BloodType,
    int? CountryGeonameId, int? StateGeonameId, int? CityGeonameId,
    string? CountryNameEn, string? CountryNameAr,
    string? StateNameEn,   string? StateNameAr,
    string? CityNameEn,    string? CityNameAr,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    string? CreatedBy, string? UpdatedBy,
    List<string> Phones, List<PatientDiseaseRow> Diseases
);

/// <summary>
/// Admin-only extension — adds clinic context (ClinicId + ClinicName).
/// SuperAdmin needs this to identify which clinic the patient belongs to.
/// </summary>
public record AdminPatientDetailData(
    Guid Id, string PatientCode,
    string FullName,
    DateOnly? DateOfBirth,
    string Gender, string? BloodType,
    int? CountryGeonameId, int? StateGeonameId, int? CityGeonameId,
    string? CountryNameEn, string? CountryNameAr,
    string? StateNameEn,   string? StateNameAr,
    string? CityNameEn,    string? CityNameAr,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    string? CreatedBy, string? UpdatedBy,
    Guid ClinicId, string? ClinicName,
    List<string> Phones, List<PatientDiseaseRow> Diseases
);

public record PatientDiseaseRow(string Id, string NameEn, string NameAr);
