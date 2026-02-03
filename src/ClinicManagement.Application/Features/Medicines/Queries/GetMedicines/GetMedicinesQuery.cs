using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;

public record GetMedicinesQuery(Guid ClinicBranchId, SearchablePaginationRequest? PaginationRequest = null) : IRequest<Result<PagedResult<MedicineDto>>>;