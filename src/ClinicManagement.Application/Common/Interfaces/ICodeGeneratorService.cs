namespace ClinicManagement.Application.Common.Interfaces;

public interface ICodeGeneratorService
{
    Task<string> GenerateInvoiceNumberAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<string> GenerateAppointmentNumberAsync(Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<string> GenerateMedicalFileNumberAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<string> GeneratePatientNumberAsync(Guid clinicId, CancellationToken cancellationToken = default);
}
