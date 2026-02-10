using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<ChronicDisease> ChronicDiseases { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<PatientChronicDisease> PatientChronicDiseases { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<ClinicBranchAppointmentPrice> ClinicBranchAppointmentPrices { get; }
    DbSet<AppointmentType> AppointmentTypes { get; }
    DbSet<MedicalFile> MedicalFiles { get; }
    
    // Clinic entities
    DbSet<Clinic> Clinics { get; }
    DbSet<ClinicBranch> ClinicBranches { get; }
    DbSet<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers { get; }
    DbSet<ClinicOwner> ClinicOwners { get; }
    
    // Staff entities
    DbSet<StaffInvitation> StaffInvitations { get; }
    DbSet<Receptionist> Receptionists { get; }
    
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
