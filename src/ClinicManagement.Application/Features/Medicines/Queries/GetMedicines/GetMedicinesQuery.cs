using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;

public record GetMedicinesQuery : IRequest<Result<List<MedicineDto>>>
{
    public string? SearchTerm { get; set; }
}
