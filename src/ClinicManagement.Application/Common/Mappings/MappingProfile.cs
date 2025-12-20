using AutoMapper;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User Mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName));
        
        CreateMap<RegisterCommand, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.ClinicId, opt => opt.Ignore());

        // Clinic Mappings (needed for user context)
        CreateMap<Clinic, ClinicDto>();
    }
}
