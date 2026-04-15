using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

/// <summary>Clinic owner locks/unlocks a doctor's ability to self-manage their schedule.</summary>
public record SetDoctorScheduleLockCommand(Guid StaffId, bool CanSelfManage) : IRequest<Result>;
