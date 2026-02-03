using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.DeleteMedicine;

public record DeleteMedicineCommand(Guid Id) : IRequest<Result>;