using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalSupplies.Queries.GetMedicalSupplies;

public record GetMedicalSuppliesQuery(Guid ClinicBranchId) : IRequest<Result<IEnumerable<MedicalSupplyDto>>>;