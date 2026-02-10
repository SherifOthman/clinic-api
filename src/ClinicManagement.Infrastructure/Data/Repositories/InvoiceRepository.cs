using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    protected override IQueryable<Invoice> ApplySearchAndFilters(IQueryable<Invoice> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(i => i.InvoiceNumber.Contains(request.SearchTerm));
        }

        // Apply filters from dictionary
        if (request.Filters != null && request.Filters.Count > 0)
        {
            if (request.Filters.TryGetValue("clinicId", out var clinicIdObj) && clinicIdObj is Guid clinicId)
            {
                query = query.Where(i => i.ClinicId == clinicId);
            }

            if (request.Filters.TryGetValue("patientId", out var patientIdObj) && patientIdObj is Guid patientId)
            {
                query = query.Where(i => i.PatientId == patientId);
            }

            if (request.Filters.TryGetValue("status", out var statusObj) && statusObj is int status)
            {
                query = query.Where(i => (int)i.Status == status);
            }

            if (request.Filters.TryGetValue("fromDate", out var fromDateObj) && fromDateObj is DateTime fromDate)
            {
                query = query.Where(i => i.IssuedDate >= fromDate);
            }

            if (request.Filters.TryGetValue("toDate", out var toDateObj) && toDateObj is DateTime toDate)
            {
                query = query.Where(i => i.IssuedDate <= toDate);
            }
        }

        return query;
    }

    protected override IQueryable<Invoice> ApplySorting(IQueryable<Invoice> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "invoicenumber" or "number" => request.IsAscending 
                ? query.OrderBy(i => i.InvoiceNumber) 
                : query.OrderByDescending(i => i.InvoiceNumber),
            "issueddate" or "date" => request.IsAscending 
                ? query.OrderBy(i => i.IssuedDate) 
                : query.OrderByDescending(i => i.IssuedDate),
            "duedate" => request.IsAscending 
                ? query.OrderBy(i => i.DueDate) 
                : query.OrderByDescending(i => i.DueDate),
            "totalamount" or "amount" => request.IsAscending 
                ? query.OrderBy(i => i.TotalAmount) 
                : query.OrderByDescending(i => i.TotalAmount),
            "status" => request.IsAscending 
                ? query.OrderBy(i => i.Status) 
                : query.OrderByDescending(i => i.Status),
            "createdat" => request.IsAscending 
                ? query.OrderBy(i => i.CreatedAt) 
                : query.OrderByDescending(i => i.CreatedAt),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };
    }
}
