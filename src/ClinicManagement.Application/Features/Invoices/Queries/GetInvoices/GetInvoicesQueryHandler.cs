using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Invoices.Queries.GetInvoices;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<PagedResult<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvoicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        PagedResult<Invoice> invoices;
        
        if (request.PaginationRequest != null)
        {
            invoices = await _unitOfWork.Invoices.GetByClinicIdPagedAsync(
                request.ClinicId, 
                request.PaginationRequest, 
                cancellationToken);
        }
        else
        {
            var allInvoices = await _unitOfWork.Invoices.GetByClinicIdAsync(request.ClinicId, cancellationToken);
            invoices = new PagedResult<Invoice>(allInvoices.ToList(), allInvoices.Count(), 1, allInvoices.Count());
        }
        
        var invoicesDto = invoices.Items.Adapt<List<InvoiceDto>>();
        var result = new PagedResult<InvoiceDto>(invoicesDto, invoices.TotalCount, invoices.PageNumber, invoices.PageSize);
        
        return Result<PagedResult<InvoiceDto>>.Success(result);
    }
}