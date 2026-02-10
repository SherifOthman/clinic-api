using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(Guid ClinicId, PaginationRequest? PaginationRequest) : IRequest<Result<PagedResult<InvoiceDto>>>;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<PagedResult<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvoicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        PagedResult<InvoiceDto> result;
        
        if (request.PaginationRequest != null)
        {
            var pagedResult = await _unitOfWork.Invoices.GetPagedByClinicAsync(
                request.ClinicId,
                request.PaginationRequest,
                cancellationToken);
            
            var invoicesDto = pagedResult.Items.Adapt<List<InvoiceDto>>();
            result = new PagedResult<InvoiceDto>(invoicesDto, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
        }
        else
        {
            var allInvoices = await _unitOfWork.Invoices.GetByClinicAsync(request.ClinicId, cancellationToken);
            var invoicesDto = allInvoices.Adapt<List<InvoiceDto>>();
            result = new PagedResult<InvoiceDto>(invoicesDto, invoicesDto.Count, 1, invoicesDto.Count);
        }
        
        return Result<PagedResult<InvoiceDto>>.Ok(result);
    }
}