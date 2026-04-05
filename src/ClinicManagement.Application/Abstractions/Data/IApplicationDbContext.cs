using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClinicManagement.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    // Identity
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<IdentityUserRole<Guid>> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<StaffInvitation> StaffInvitations { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<EmailQueue> EmailQueue { get; }
    DbSet<AuditLog> AuditLogs { get; }

    // Reference Data
    DbSet<Specialization> Specializations { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<ChronicDisease> ChronicDiseases { get; }

    // Clinic
    DbSet<Clinic> Clinics { get; }
    DbSet<ClinicBranch> ClinicBranches { get; }
    DbSet<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers { get; }
    DbSet<ClinicBranchAppointmentPrice> ClinicBranchAppointmentPrices { get; }
    DbSet<ClinicSubscription> ClinicSubscriptions { get; }
    DbSet<ClinicUsageMetrics> ClinicUsageMetrics { get; }
    DbSet<DoctorWorkingDay> DoctorWorkingDays { get; }
    DbSet<SubscriptionPayment> SubscriptionPayments { get; }

    // Staff
    DbSet<Domain.Entities.Staff> Staff { get; }
    DbSet<DoctorProfile> DoctorProfiles { get; }

    // Patient
    DbSet<Patient> Patients { get; }
    DbSet<PatientPhone> PatientPhones { get; }
    DbSet<PatientChronicDisease> PatientChronicDiseases { get; }

    // Appointment
    DbSet<Appointment> Appointments { get; }
    DbSet<AppointmentType> AppointmentTypes { get; }

    // Medical
    DbSet<MedicalVisit> MedicalVisits { get; }
    DbSet<MedicalFile> MedicalFiles { get; }
    DbSet<PatientMedicalFile> PatientMedicalFiles { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<LabTest> LabTests { get; }
    DbSet<LabTestOrder> LabTestOrders { get; }
    DbSet<MedicalVisitLabTest> MedicalVisitLabTests { get; }
    DbSet<RadiologyTest> RadiologyTests { get; }
    DbSet<RadiologyOrder> RadiologyOrders { get; }
    DbSet<MedicalVisitRadiology> MedicalVisitRadiologies { get; }
    DbSet<MeasurementAttribute> MeasurementAttributes { get; }
    DbSet<MedicalVisitMeasurement> MedicalVisitMeasurements { get; }
    DbSet<DoctorMeasurementAttribute> DoctorMeasurementAttributes { get; }
    DbSet<SpecializationMeasurementAttribute> SpecializationMeasurementAttributes { get; }

    DbSet<Medicine> Medicines { get; }
    DbSet<MedicineDispensing> MedicineDispensings { get; }
    DbSet<MedicalService> MedicalServices { get; }
    DbSet<MedicalSupply> MedicalSupplies { get; }

    // Billing
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Payment> Payments { get; }

    // SaveChanges
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
