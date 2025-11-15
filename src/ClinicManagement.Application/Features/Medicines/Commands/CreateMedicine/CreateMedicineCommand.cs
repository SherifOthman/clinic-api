using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;

public record CreateMedicineCommand : IRequest<Result<MedicineDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Dosage { get; set; }
    public string? Form { get; set; }
    public string? Description { get; set; }
}
