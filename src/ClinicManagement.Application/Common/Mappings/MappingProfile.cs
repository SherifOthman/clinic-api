using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

/// <summary>
/// Centralized mapping configuration for the application.
/// Only contains mappings that are actually used.
/// </summary>
public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // ChronicDisease to ChronicDiseaseDto mapping
        // Maps language-specific fields and sets default display values
        TypeAdapterConfig<ChronicDisease, ChronicDiseaseDto>
            .NewConfig()
            .Map(dest => dest.Name, src => src.NameEn) // Default to English
            .Map(dest => dest.Description, src => src.DescriptionEn); // Default to English
        
        // Specialization to SpecializationDto mapping
        // Automatic mapping since property names match
        TypeAdapterConfig<Specialization, SpecializationDto>
            .NewConfig();
        
        // Doctor to DoctorDto mapping
        // Includes specialization navigation property
        TypeAdapterConfig<Doctor, DoctorDto>
            .NewConfig()
            .Map(dest => dest.Specialization, src => src.Specialization);
        
        // PatientChronicDisease to PatientChronicDiseaseDto mapping
        // Includes chronic disease navigation property
        TypeAdapterConfig<PatientChronicDisease, PatientChronicDiseaseDto>
            .NewConfig()
            .Map(dest => dest.ChronicDisease, src => src.ChronicDisease);
        
        // Appointment to AppointmentDto mapping
        // Includes patient and doctor names
        TypeAdapterConfig<Appointment, AppointmentDto>
            .NewConfig()
            .Map(dest => dest.PatientName, src => src.Patient.FullName)
            .Map(dest => dest.DoctorName, src => $"{src.Doctor.User.FirstName} {src.Doctor.User.LastName}".Trim())
            .Map(dest => dest.AppointmentType, src => src.AppointmentType)
            .Map(dest => dest.RemainingAmount, src => src.FinalPrice - src.DiscountAmount - src.PaidAmount);
        
        // User to UserDto mapping is handled automatically by Mapster
        // since property names match between User entity and UserDto
        
        // New pharmacy and billing mappings
        TypeAdapterConfig<Medicine, MedicineDto>
            .NewConfig()
            .Map(dest => dest.StripPrice, src => src.StripPrice)
            .Map(dest => dest.FullBoxesInStock, src => src.FullBoxesInStock)
            .Map(dest => dest.RemainingStrips, src => src.RemainingStrips)
            .Map(dest => dest.IsLowStock, src => src.IsLowStock)
            .Map(dest => dest.HasStock, src => src.HasStock);

        TypeAdapterConfig<MedicalSupply, MedicalSupplyDto>
            .NewConfig()
            .Map(dest => dest.IsLowStock, src => src.IsLowStock)
            .Map(dest => dest.HasStock, src => src.HasStock);

        TypeAdapterConfig<MedicalService, MedicalServiceDto>
            .NewConfig();

        TypeAdapterConfig<Invoice, InvoiceDto>
            .NewConfig()
            .Map(dest => dest.FinalAmount, src => src.FinalAmount)
            .Map(dest => dest.TotalPaid, src => src.TotalPaid)
            .Map(dest => dest.RemainingAmount, src => src.RemainingAmount)
            .Map(dest => dest.IsFullyPaid, src => src.IsFullyPaid)
            .Map(dest => dest.IsOverdue, src => src.IsOverdue);

        TypeAdapterConfig<InvoiceItem, InvoiceItemDto>
            .NewConfig()
            .Map(dest => dest.LineTotal, src => src.LineTotal);

        TypeAdapterConfig<Payment, PaymentDto>
            .NewConfig();

        // Measurement mappings
        TypeAdapterConfig<MeasurementAttribute, MeasurementAttributeDto>
            .NewConfig();
        
        TypeAdapterConfig<MedicalVisitMeasurement, MedicalVisitMeasurementDto>
            .NewConfig()
            .Map(dest => dest.MeasurementName, src => src.MeasurementAttribute.NameEn)
            .Map(dest => dest.Value, src => src.GetValue());

        TypeAdapterConfig<DoctorMeasurementAttribute, DoctorMeasurementAttributeDto>
            .NewConfig()
            .Map(dest => dest.MeasurementName, src => src.MeasurementAttribute.NameEn)
            .Map(dest => dest.DataType, src => src.MeasurementAttribute.DataType.ToString());
    }
}
