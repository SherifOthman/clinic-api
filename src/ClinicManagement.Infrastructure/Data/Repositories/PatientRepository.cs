using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.ClinicId == clinicId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Patient>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet
            .Where(p => p.FirstName.ToLower().Contains(lowerSearchTerm) ||
                       (p.SecondName != null && p.SecondName.ToLower().Contains(lowerSearchTerm)) ||
                       p.ThirdName.ToLower().Contains(lowerSearchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.PhoneNumber == phoneNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Patient?> GetWithSurgeriesAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Surgeries)
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
    }
}

