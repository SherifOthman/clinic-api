using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands;

public record SetBranchActiveStatusCommand(Guid Id, bool IsActive) : IRequest<Result>;
