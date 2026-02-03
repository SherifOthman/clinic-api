using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(Guid ClinicId) : IRequest<Result<IEnumerable<InvoiceDto>>>;