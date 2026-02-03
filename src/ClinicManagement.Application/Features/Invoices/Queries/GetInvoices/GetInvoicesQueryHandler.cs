using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Invoices.Queries.GetInvoices;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<IEnumerable<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvoicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _unitOfWork.Invoices.GetByClinicIdAsync(request.ClinicId, cancellationToken);
        var invoicesDto = invoices.Adapt<IEnumerable<InvoiceDto>>();
        
        return Result<IEnumerable<InvoiceDto>>.Success(invoicesDto);
    }
}