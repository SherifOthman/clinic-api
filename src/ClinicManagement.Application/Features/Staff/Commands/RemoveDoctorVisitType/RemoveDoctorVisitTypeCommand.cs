using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record RemoveDoctorVisitTypeCommand(Guid VisitTypeId) : IRequest<Result>;
