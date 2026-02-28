using ClinicManagement.Application.Features.Reference.Queries;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public class ChronicDiseaseMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ChronicDisease, ChronicDiseaseDto>();
    }
}
