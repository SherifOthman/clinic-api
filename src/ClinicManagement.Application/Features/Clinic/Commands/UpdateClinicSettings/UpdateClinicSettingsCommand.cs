using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicSettings.Commands;

/// <summary>Clinic owner updates clinic-level settings (e.g. week start day).</summary>
public record UpdateClinicSettingsCommand(
    int WeekStartDay   // 0 = Sunday … 6 = Saturday
) : IRequest<Result>;
