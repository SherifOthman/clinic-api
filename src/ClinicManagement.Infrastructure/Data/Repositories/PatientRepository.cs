using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Extensions;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class PatientRepository : BaseRepository<Patient>, IPatientRepository
{
    private readonly ICurrentUserService _currentUserService;

    public PatientRepository(ApplicationDbContext context, ICurrentUserService currentUserService) : base(context)
    {
        _currentUserService = currentUserService;
    }

    public override async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
                .ThenInclude(pcd => pcd.ChronicDisease)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<PagedResult<Patient>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
                .ThenInclude(pcd => pcd.ChronicDisease)
            .AsQueryable();

        if (request is PatientSearchRequest patientRequest)
        {
            if (patientRequest.HasSearchTerm)
            {
                var lowerSearchTerm = patientRequest.SearchTerm!.ToLower();
                query = query.Where(p => 
                    p.FullName.ToLower().Contains(lowerSearchTerm) ||
                    p.PhoneNumbers.Any(pn => pn.PhoneNumber.Contains(patientRequest.SearchTerm!)));
            }

            if (patientRequest.HasGenderFilter)
            {
                query = query.Where(p => p.Gender == patientRequest.Gender!.Value);
            }

            if (patientRequest.HasDateOfBirthFilter)
            {
                if (patientRequest.DateOfBirthFrom.HasValue)
                {
                    query = query.Where(p => p.DateOfBirth >= patientRequest.DateOfBirthFrom.Value);
                }
                if (patientRequest.DateOfBirthTo.HasValue)
                {
                    query = query.Where(p => p.DateOfBirth <= patientRequest.DateOfBirthTo.Value);
                }
            }

            if (patientRequest.HasCreatedDateFilter)
            {
                if (patientRequest.CreatedFrom.HasValue)
                {
                    query = query.Where(p => p.CreatedAt >= patientRequest.CreatedFrom.Value);
                }
                if (patientRequest.CreatedTo.HasValue)
                {
                    query = query.Where(p => p.CreatedAt <= patientRequest.CreatedTo.Value);
                }
            }

            if (patientRequest.HasAgeFilter)
            {
                var today = DateTime.Today;
                if (patientRequest.MinAge.HasValue)
                {
                    var maxBirthDate = today.AddYears(-patientRequest.MinAge.Value);
                    query = query.Where(p => p.DateOfBirth.HasValue && p.DateOfBirth <= maxBirthDate);
                }
                if (patientRequest.MaxAge.HasValue)
                {
                    var minBirthDate = today.AddYears(-patientRequest.MaxAge.Value - 1);
                    query = query.Where(p => p.DateOfBirth.HasValue && p.DateOfBirth >= minBirthDate);
                }
            }

            query = patientRequest.SortBy?.ToLower() switch
            {
                "fullname" or "name" => patientRequest.SortDescending 
                    ? query.OrderByDescending(p => p.FullName) 
                    : query.OrderBy(p => p.FullName),
                "age" => patientRequest.SortDescending 
                    ? query.OrderBy(p => p.DateOfBirth ?? DateTime.MinValue) 
                    : query.OrderByDescending(p => p.DateOfBirth ?? DateTime.MinValue),
                "createdat" => patientRequest.SortDescending 
                    ? query.OrderByDescending(p => p.CreatedAt) 
                    : query.OrderBy(p => p.CreatedAt),
                "gender" => patientRequest.SortDescending 
                    ? query.OrderByDescending(p => p.Gender) 
                    : query.OrderBy(p => p.Gender),
                _ => query.OrderBy(p => p.FullName)
            };
        }
        else
        {
            query = query.OrderBy(p => p.FullName);
        }

        return await query.ToPaginatedResultAsync(request, cancellationToken);
    }
}
