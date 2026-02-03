using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(Guid ClinicId, SearchablePaginationRequest? PaginationRequest = null) : IRequest<Result<PagedResult<InvoiceDto>>>;