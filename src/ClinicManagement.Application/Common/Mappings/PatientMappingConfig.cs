using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public class PatientMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Patient, PatientDto>()
            .Map(dest => dest.Id, src => src.Id.ToString())
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth.ToString("yyyy-MM-dd"))
            .Map(dest => dest.Age, src => DateTime.UtcNow.Year - src.DateOfBirth.Year)
            .Map(dest => dest.BloodType, src => src.BloodType.HasValue ? src.BloodType.Value.ToString() : null)
            .Map(dest => dest.PhoneNumbers, src => src.Phones.Select(ph => ph.PhoneNumber).ToList())
            .Map(dest => dest.PrimaryPhone, src => 
                src.Phones.Where(ph => ph.IsPrimary).Select(ph => ph.PhoneNumber).FirstOrDefault() 
                ?? src.Phones.OrderBy(ph => ph.CreatedAt).Select(ph => ph.PhoneNumber).FirstOrDefault())
            .Map(dest => dest.CreatedAt, src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}
