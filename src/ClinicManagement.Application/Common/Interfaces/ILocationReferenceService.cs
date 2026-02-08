namespace ClinicManagement.Application.Common.Interfaces;

public interface ILocationReferenceService
{
    Task ResolveLocationAsync(
        int countryGeonameId,
        CountrySnapshotData countryData,
        int stateGeonameId,
        StateSnapshotData stateData,
        int cityGeonameId,
        CitySnapshotData cityData,
        CancellationToken cancellationToken = default);
}

public record CountrySnapshotData(string Iso2Code, string PhoneCode, string NameEn, string NameAr);

public record StateSnapshotData(string NameEn, string NameAr);

public record CitySnapshotData(string NameEn, string NameAr);
