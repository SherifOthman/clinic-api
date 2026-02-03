using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Staff> Staff { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<ChronicDisease> ChronicDiseases { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<ClinicPatient> ClinicPatients { get; }
    DbSet<ClinicPatientChronicDisease> ClinicPatientChronicDiseases { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<ClinicBranchAppointmentPrice> ClinicBranchAppointmentPrices { get; }
    DbSet<AppointmentType> AppointmentTypes { get; }
    DbSet<MedicalFile> MedicalFiles { get; }
    
    // New pharmacy and billing entities
    DbSet<Medicine> Medicines { get; }
    DbSet<MedicalSupply> MedicalSupplies { get; }
    DbSet<MedicalService> MedicalServices { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Payment> Payments { get; }
    
    // Measurement entities
    DbSet<MeasurementAttribute> MeasurementAttributes { get; }
    DbSet<MedicalVisitMeasurement> MedicalVisitMeasurements { get; }
    DbSet<DoctorMeasurementAttribute> DoctorMeasurementAttributes { get; }
    DbSet<SpecializationMeasurementAttribute> SpecializationMeasurementAttributes { get; }
    
    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
