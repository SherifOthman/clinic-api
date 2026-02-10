using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(Guid ClinicId, PaginationRequest? PaginationRequest) : IRequest<Result<PagedResult<InvoiceDto>>>;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<PagedResult<InvoiceDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Where(i => i.ClinicId == request.ClinicId)
            .OrderByDescending(i => i.CreatedAt);

        PagedResult<InvoiceDto> result;
        
        if (request.PaginationRequest != null)
        {
            var totalCount = await query.CountAsync(cancellationToken);
            
            var invoices = await query
                .Skip((request.PaginationRequest.PageNumber - 1) * request.PaginationRequest.PageSize)
                .Take(request.PaginationRequest.PageSize)
                .ToListAsync(cancellationToken);
            
            var invoicesDto = invoices.Adapt<List<InvoiceDto>>();
            result = new PagedResult<InvoiceDto>(invoicesDto, totalCount, request.PaginationRequest.PageNumber, request.PaginationRequest.PageSize);
        }
        else
        {
            var allInvoices = await query.ToListAsync(cancellationToken);
            var invoicesDto = allInvoices.Adapt<List<InvoiceDto>>();
            result = new PagedResult<InvoiceDto>(invoicesDto, invoicesDto.Count, 1, invoicesDto.Count);
        }
        
        return Result<PagedResult<InvoiceDto>>.Ok(result);
    }
}