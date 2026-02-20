namespace ClinicManagement.Application.Abstractions.Services;

public interface ICodeGeneratorService
{
    string GeneratePatientCode(int clinicId, int patientId);
    string GenerateInvoiceNumber(int clinicId, int invoiceId, DateTime date);
    string GenerateAppointmentNumber(int clinicId, int appointmentId, DateTime date);
    string GenerateMedicalFileNumber(int clinicId, int fileId, DateTime date);
    string GeneratePrescriptionNumber(int clinicId, int prescriptionId, DateTime date);
}
