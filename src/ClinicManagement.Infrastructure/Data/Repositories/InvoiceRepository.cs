using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Invoice>> GetByClinicIdPagedAsync(Guid clinicId, SearchablePaginationRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<Invoice> query = _dbSet
            .AsNoTracking()
            .Where(i => i.ClinicId == clinicId)
            .Include(i => i.Items)
            .Include(i => i.Payments);

        // Apply search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(i => i.Patient.FullName.Contains(request.SearchTerm));
        }

        // Apply filters
        if (request.Filters.ContainsKey("status") && Enum.TryParse<InvoiceStatus>(request.Filters["status"].ToString(), out var status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (request.Filters.ContainsKey("isOverdue") && bool.TryParse(request.Filters["isOverdue"].ToString(), out bool isOverdue))
        {
            if (isOverdue)
            {
                query = query.Where(i => i.DueDate.HasValue && i.DueDate < DateTime.UtcNow && i.RemainingAmount > 0);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "totalamount" => request.IsAscending ? query.OrderBy(i => i.TotalAmount) : query.OrderByDescending(i => i.TotalAmount),
            "finalamount" => request.IsAscending ? query.OrderBy(i => i.FinalAmount) : query.OrderByDescending(i => i.FinalAmount),
            "status" => request.IsAscending ? query.OrderBy(i => i.Status) : query.OrderByDescending(i => i.Status),
            "duedate" => request.IsAscending ? query.OrderBy(i => i.DueDate) : query.OrderByDescending(i => i.DueDate),
            "createdat" => request.IsAscending ? query.OrderBy(i => i.CreatedAt) : query.OrderByDescending(i => i.CreatedAt),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Invoice>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<IEnumerable<Invoice>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ClinicId == clinicId)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.ClinicId == clinicId, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByPatientIdAsync(Guid PatientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.PatientId == PatientId)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ClinicId == clinicId && i.RemainingAmount > 0)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .OrderBy(i => i.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetWithItemsAndPaymentsAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(i => i.Items)
                .ThenInclude(item => item.MedicalService)
            .Include(i => i.Items)
                .ThenInclude(item => item.Medicine)
            .Include(i => i.Items)
                .ThenInclude(item => item.MedicalSupply)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.ClinicId == clinicId, cancellationToken);
    }
}
