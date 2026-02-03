using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;

public record GetMedicinesQuery(Guid ClinicBranchId) : IRequest<Result<IEnumerable<MedicineDto>>>;