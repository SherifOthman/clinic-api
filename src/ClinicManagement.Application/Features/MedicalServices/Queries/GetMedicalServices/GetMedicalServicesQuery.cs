using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalServices.Queries.GetMedicalServices;

public record GetMedicalServicesQuery(Guid ClinicBranchId) : IRequest<Result<IEnumerable<MedicalServiceDto>>>;