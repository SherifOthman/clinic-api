using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicine;

public record GetMedicineQuery(Guid Id) : IRequest<Result<MedicineDto>>;
