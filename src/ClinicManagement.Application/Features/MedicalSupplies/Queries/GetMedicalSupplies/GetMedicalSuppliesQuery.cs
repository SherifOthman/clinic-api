using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalSupplies.Queries.GetMedicalSupplies;

public record GetMedicalSuppliesQuery(Guid ClinicBranchId, SearchablePaginationRequest? PaginationRequest = null) : IRequest<Result<PagedResult<MedicalSupplyDto>>>;