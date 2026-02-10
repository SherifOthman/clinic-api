using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    protected override IQueryable<Payment> ApplySearchAndFilters(IQueryable<Payment> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => 
                (p.ReferenceNumber != null && p.ReferenceNumber.Contains(request.SearchTerm)) ||
                (p.Note != null && p.Note.Contains(request.SearchTerm)));
        }

        // Apply filters from dictionary
        if (request.Filters != null && request.Filters.Count > 0)
        {
            if (request.Filters.TryGetValue("invoiceId", out var invoiceIdObj) && invoiceIdObj is Guid invoiceId)
            {
                query = query.Where(p => p.InvoiceId == invoiceId);
            }

            if (request.Filters.TryGetValue("paymentMethod", out var methodObj) && methodObj is int method)
            {
                query = query.Where(p => (int)p.PaymentMethod == method);
            }

            if (request.Filters.TryGetValue("status", out var statusObj) && statusObj is int status)
            {
                query = query.Where(p => (int)p.Status == status);
            }

            if (request.Filters.TryGetValue("fromDate", out var fromDateObj) && fromDateObj is DateTime fromDate)
            {
                query = query.Where(p => p.PaymentDate >= fromDate);
            }

            if (request.Filters.TryGetValue("toDate", out var toDateObj) && toDateObj is DateTime toDate)
            {
                query = query.Where(p => p.PaymentDate <= toDate);
            }
        }

        return query;
    }

    protected override IQueryable<Payment> ApplySorting(IQueryable<Payment> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "paymentdate" or "date" => request.IsAscending 
                ? query.OrderBy(p => p.PaymentDate) 
                : query.OrderByDescending(p => p.PaymentDate),
            "amount" => request.IsAscending 
                ? query.OrderBy(p => p.Amount) 
                : query.OrderByDescending(p => p.Amount),
            "paymentmethod" or "method" => request.IsAscending 
                ? query.OrderBy(p => p.PaymentMethod) 
                : query.OrderByDescending(p => p.PaymentMethod),
            "status" => request.IsAscending 
                ? query.OrderBy(p => p.Status) 
                : query.OrderByDescending(p => p.Status),
            "referencenumber" or "reference" => request.IsAscending 
                ? query.OrderBy(p => p.ReferenceNumber) 
                : query.OrderByDescending(p => p.ReferenceNumber),
            "createdat" => request.IsAscending 
                ? query.OrderBy(p => p.CreatedAt) 
                : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.PaymentDate)
        };
    }
}
