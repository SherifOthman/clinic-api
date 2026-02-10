using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;

namespace ClinicManagement.Infrastructure.Services;

public class CodeGeneratorService : ICodeGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;

    public CodeGeneratorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GenerateInvoiceNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _unitOfWork.Invoices.GetCountForClinicByYearAsync(clinicId, year, cancellationToken);
        
        return $"INV-{year}-{(count + 1):D3}";
    }

    public async Task<string> GenerateAppointmentNumberAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _unitOfWork.Appointments.GetCountForClinicBranchByYearAsync(clinicBranchId, year, cancellationToken);
        
        return $"APT-{year}-{(count + 1):D3}";
    }

    public async Task<string> GenerateMedicalFileNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _unitOfWork.MedicalFiles.GetCountForClinicByYearAsync(clinicId, year, cancellationToken);
        
        return $"MF-{year}-{(count + 1):D3}";
    }

    public async Task<string> GeneratePatientNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _unitOfWork.Patients.GetCountForClinicByYearAsync(clinicId, year, cancellationToken);
        
        return $"PAT-{year}-{(count + 1):D4}";
    }
}
