using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Clinics.Commands.SetupClinic;

public record SetupClinicCommand : IRequest<Result<ClinicDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int OwnerId { get; set; }
    public int SubscriptionPlanId { get; set; }
}
