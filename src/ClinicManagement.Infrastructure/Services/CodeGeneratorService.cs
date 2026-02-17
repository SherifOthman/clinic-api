using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Services;

public class CodeGeneratorService
{
    private readonly ApplicationDbContext _db;
    private readonly CurrentUserService _currentUser;
    private readonly DateTimeProvider _dateTimeProvider;

    public CodeGeneratorService(ApplicationDbContext db, CurrentUserService currentUser, DateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var clinicId = _currentUser.ClinicId!.Value;
        var year = _dateTimeProvider.UtcNow.Year;
        var count = await _db.Invoices
            .CountAsync(i => i.ClinicId == clinicId && i.CreatedAt.Year == year, cancellationToken);
        
        return $"INV-{year}-{(count + 1):D6}";
    }

    public async Task<string> GenerateAppointmentNumberAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        var year = _dateTimeProvider.UtcNow.Year;
        var count = await _db.Appointments
            .CountAsync(a => a.ClinicBranchId == clinicBranchId && a.CreatedAt.Year == year, cancellationToken);
        
        return $"APT-{year}-{(count + 1):D6}";
    }

    public async Task<string> GenerateMedicalFileNumberAsync(CancellationToken cancellationToken = default)
    {
        var clinicId = _currentUser.ClinicId!.Value;
        var year = _dateTimeProvider.UtcNow.Year;
        // MedicalFiles table doesn't exist yet, return placeholder
        return $"MF-{year}-{1:D6}";
    }

    public async Task<string> GeneratePatientNumberAsync(CancellationToken cancellationToken = default)
    {
        var clinicId = _currentUser.ClinicId!.Value;
        var year = _dateTimeProvider.UtcNow.Year;
        var count = await _db.Patients
            .CountAsync(p => p.ClinicId == clinicId && p.CreatedAt.Year == year, cancellationToken);
        
        return $"PAT-{year}-{(count + 1):D6}";
    }
}
