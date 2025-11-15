using AutoMapper;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<RegisterCommand, User>();
        
        CreateMap<Country, CountryDto>();
        CreateMap<CreateCountryDto, Country>();
        
        CreateMap<Governorate, GovernorateDto>();
        CreateMap<CreateGovernorateDto, Governorate>();
        
        CreateMap<City, CityDto>();
        CreateMap<CreateCityDto, City>();
        
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>();
        CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>();
        
        CreateMap<Clinic, ClinicDto>();
        CreateMap<CreateClinicDto, Clinic>();
        CreateMap<UpdateClinicDto, Clinic>();
        
        CreateMap<ClinicBranch, ClinicBranchDto>();
        CreateMap<CreateClinicBranchDto, ClinicBranch>();
        
        CreateMap<Specialization, SpecializationDto>();
        CreateMap<CreateSpecializationDto, Specialization>();
        
        CreateMap<Doctor, DoctorDto>();
        CreateMap<CreateDoctorDto, Doctor>();
        
        CreateMap<Patient, PatientDto>();
        CreateMap<CreatePatientDto, Patient>();
        CreateMap<UpdatePatientDto, Patient>();
        
        CreateMap<Appointment, AppointmentDto>();
        CreateMap<CreateAppointmentDto, Appointment>();
        CreateMap<UpdateAppointmentDto, Appointment>();
        
        CreateMap<Visit, VisitDto>();
        CreateMap<CreateVisitDto, Visit>();
        
        CreateMap<Medicine, MedicineDto>();
        CreateMap<CreateMedicineDto, Medicine>();
        
        CreateMap<PrescriptionMedicine, PrescriptionMedicineDto>();
        CreateMap<CreatePrescriptionMedicineDto, PrescriptionMedicine>();
        
        CreateMap<Review, ReviewDto>();
        CreateMap<CreateReviewDto, Review>();
    }
}
