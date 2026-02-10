using ClinicManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Services;

public class CodeGeneratorService : ICodeGeneratorService
{
    private readonly IApplicationDbContext _context;

    public CodeGeneratorService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateInvoiceNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Invoices
            .Where(i => i.ClinicId == clinicId && i.CreatedAt.Year == year)
            .CountAsync(cancellationToken);
        
        return $"INV-{year}-{(count + 1):D3}";
    }

    public async Task<string> GenerateAppointmentNumberAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Appointments
            .Where(a => a.ClinicBranchId == clinicBranchId && a.CreatedAt.Year == year)
            .CountAsync(cancellationToken);
        
        return $"APT-{year}-{(count + 1):D3}";
    }

    public async Task<string> GenerateMedicalFileNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.MedicalFiles
            .Where(mf => mf.Patient.ClinicId == clinicId && mf.UploadedAt.Year == year)
            .CountAsync(cancellationToken);
        
        return $"MF-{year}-{(count + 1):D3}";
    }

    public async Task<string> GeneratePatientNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Patients
            .Where(cp => cp.ClinicId == clinicId && cp.CreatedAt.Year == year)
            .CountAsync(cancellationToken);
        
        return $"PAT-{year}-{(count + 1):D4}";
    }
}
