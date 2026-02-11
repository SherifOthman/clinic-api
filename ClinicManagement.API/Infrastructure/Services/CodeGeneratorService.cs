using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Infrastructure.Services;

public class CodeGeneratorService
{
    private readonly ApplicationDbContext _db;

    public CodeGeneratorService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateInvoiceNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _db.Invoices
            .CountAsync(i => i.ClinicId == clinicId && i.CreatedAt.Year == year, cancellationToken);
        
        return $"INV-{year}-{(count + 1):D6}";
    }

    public async Task<string> GenerateAppointmentNumberAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _db.Appointments
            .CountAsync(a => a.ClinicBranchId == clinicBranchId && a.CreatedAt.Year == year, cancellationToken);
        
        return $"APT-{year}-{(count + 1):D6}";
    }

    public async Task<string> GenerateMedicalFileNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        // MedicalFiles table doesn't exist yet, return placeholder
        return $"MF-{year}-{1:D6}";
    }

    public async Task<string> GeneratePatientNumberAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _db.Patients
            .CountAsync(p => p.ClinicId == clinicId && p.CreatedAt.Year == year, cancellationToken);
        
        return $"PAT-{year}-{(count + 1):D6}";
    }
}
