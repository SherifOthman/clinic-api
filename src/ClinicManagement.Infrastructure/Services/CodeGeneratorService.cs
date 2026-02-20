using ClinicManagement.Application.Abstractions.Services;

namespace ClinicManagement.Infrastructure.Services;

public class CodeGeneratorService : ICodeGeneratorService
{
    public string GeneratePatientCode(int clinicId, int patientId)
    {
        return $"P{clinicId:D4}-{patientId:D6}";
    }

    public string GenerateInvoiceNumber(int clinicId, int invoiceId, DateTime date)
    {
        return $"INV{clinicId:D4}-{date:yyyyMM}-{invoiceId:D5}";
    }

    public string GenerateAppointmentNumber(int clinicId, int appointmentId, DateTime date)
    {
        return $"APT{clinicId:D4}-{date:yyyyMMdd}-{appointmentId:D4}";
    }

    public string GenerateMedicalFileNumber(int clinicId, int fileId, DateTime date)
    {
        return $"MF{clinicId:D4}-{date:yyyy}-{fileId:D5}";
    }

    public string GeneratePrescriptionNumber(int clinicId, int prescriptionId, DateTime date)
    {
        return $"RX{clinicId:D4}-{date:yyyyMMdd}-{prescriptionId:D4}";
    }
}
