using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Infrastructure.Extensions;
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
                       (p.MiddleName != null && p.MiddleName.ToLower().Contains(lowerSearchTerm)) ||
                       p.LastName.ToLower().Contains(lowerSearchTerm))
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

    public async Task<IEnumerable<Patient>> GetPatientsPagedAsync(int? clinicId, string? searchTerm, Gender? gender, string? city, int? minAge, int? maxAge, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (clinicId.HasValue)
            query = query.Where(p => p.ClinicId == clinicId.Value);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(searchLower) ||
                (p.MiddleName != null && p.MiddleName.ToLower().Contains(searchLower)) ||
                p.LastName.ToLower().Contains(searchLower) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm))
            );
        }

        if (gender.HasValue)
            query = query.Where(p => p.Gender == gender.Value);

        if (!string.IsNullOrEmpty(city))
            query = query.Where(p => p.City == city);

        if (minAge.HasValue)
        {
            var maxBirthDate = DateTime.Today.AddYears(-minAge.Value);
            query = query.Where(p => p.DateOfBirth.HasValue && p.DateOfBirth <= maxBirthDate);
        }

        if (maxAge.HasValue)
        {
            var minBirthDate = DateTime.Today.AddYears(-maxAge.Value - 1);
            query = query.Where(p => p.DateOfBirth.HasValue && p.DateOfBirth >= minBirthDate);
        }

        return await query
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .Paginate(pageNumber, pageSize)
            .ToListAsync(cancellationToken);
    }
}

