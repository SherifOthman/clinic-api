using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public class PatientMappingConfig : IRegister
{
    private static string? ToBloodTypeDisplay(BloodType bt) => bt switch
    {
        BloodType.APositive  => "A+",  BloodType.ANegative  => "A-",
        BloodType.BPositive  => "B+",  BloodType.BNegative  => "B-",
        BloodType.ABPositive => "AB+", BloodType.ABNegative => "AB-",
        BloodType.OPositive  => "O+",  BloodType.ONegative  => "O-",
        _ => null,
    };
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Patient, PatientDto>()
            .Map(dest => dest.Id, src => src.Id.ToString())
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth.ToString("yyyy-MM-dd"))
            .Map(dest => dest.Age, src => DateTime.UtcNow.Year - src.DateOfBirth.Year)
            .Map(dest => dest.BloodType, src => src.BloodType.HasValue ? ToBloodTypeDisplay(src.BloodType.Value) : null)
            .Map(dest => dest.PrimaryPhone, src =>
                src.Phones.Where(ph => ph.IsPrimary).Select(ph => ph.PhoneNumber).FirstOrDefault()
                ?? src.Phones.Select(ph => ph.PhoneNumber).FirstOrDefault())
            .Map(dest => dest.CreatedAt, src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"))
            .Ignore(dest => dest.PhoneCount)
            .Ignore(dest => dest.ChronicDiseaseCount);
    }
}
